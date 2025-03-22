using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using TechnoPoss.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TechnoPoss
{
    public partial class NewsPage : ContentPage
    {
        private const int PageSize = 3;
        private const int MaxNewsCards = 40; 
        private int _currentPage = 0;
        private List<NewsItem> _allNews = new();
        private bool _isLoading;
        private readonly NewsParser _newsParser = new();

        public NewsPage()
        {
            InitializeComponent();
            SetupScrollListener();
            LoadInitialNews();
        }

        private void SetupScrollListener()
        {
            const int loadThreshold = 50;
            const int loadDelay = 200;

            MainScroll.Scrolled += async (sender, e) =>
            {
                if (_isLoading || _allNews.Count == 0) return;

                if (sender is ScrollView scrollView &&
                    scrollView.ContentSize.Height > 0 &&
                    scrollView.Height > 0)
                {
                    var scrollingSpace = scrollView.ContentSize.Height - scrollView.Height;

                    if (scrollView.ScrollY >= scrollingSpace - loadThreshold)
                    {
                        _isLoading = true;
                        try
                        {
                            await Task.Delay(loadDelay);

                            int currentCardCount = NewsContainer.Children.Count - 2; 
                            if (currentCardCount >= MaxNewsCards)
                            {
                                await MainScroll.ScrollToAsync(0, 0, true);
                                await RefreshNews();
                            }
                            else
                            {
                                DisplayNewsPage();
                            }
                        }
                        finally
                        {
                            _isLoading = false;
                        }
                    }
                }
            };
        }

        private async void LoadInitialNews()
        {
            try
            {
                SetLoadingState(true);

                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    Dispatcher.Dispatch(() =>
                    {
                        NewsContainer.Children.Clear();
                        NewsContainer.Children.Add(CreateNoInternetCard());
                    });
                }
                else
                {
                    try
                    {
                        var newsItems = await _newsParser.ParseNewsAsync();
                        if (newsItems == null || newsItems.Count == 0)
                        {
                            Dispatcher.Dispatch(() =>
                            {
                                NewsContainer.Children.Clear();
                                NewsContainer.Children.Add(CreateNoInternetCard());
                            });
                        }
                        else
                        {
                            _allNews = newsItems;
                            ResetNewsView();
                        }
                    }
                    catch (Exception)
                    {
                        Dispatcher.Dispatch(() =>
                        {
                            NewsContainer.Children.Clear();
                            NewsContainer.Children.Add(CreateNoInternetCard());
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка загрузки: {ex.Message}", "OK");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void ResetNewsView()
        {
            Dispatcher.Dispatch(() =>
            {
                while (NewsContainer.Children.Count > 2)
                {
                    NewsContainer.Children.RemoveAt(2);
                }
                _currentPage = 0;
                DisplayNewsPage();
            });
        }

        private void DisplayNewsPage()
        {
            var start = _currentPage * PageSize;
            var end = Math.Min(start + PageSize, _allNews.Count);

            Dispatcher.Dispatch(() =>
            {
                for (var i = start; i < end; i++)
                {
                    AddNewsItem(_allNews[i]);
                }
                _currentPage++;
            });
        }

        private void AddNewsItem(NewsItem item)
        {
            ImageSource imageSource = GetImageSource(item.ImageUrl);

            var newsCard = new Frame
            {
                BorderColor = Colors.Transparent,
                BackgroundColor = Color.FromArgb("#2D2D2D"),
                Padding = 15,
                CornerRadius = 12,
                HasShadow = true,
                Content = new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        new Frame
                        {
                            CornerRadius = 10,
                            IsClippedToBounds = true,
                            Padding = 0,
                            Margin = 0,
                            BorderColor = Colors.Transparent,
                            BackgroundColor = Colors.Transparent,
                            Content = new Image
                            {
                                Source = imageSource,
                                Aspect = Aspect.AspectFill,
                                HeightRequest = 200,
                                HorizontalOptions = LayoutOptions.Fill,
                                Margin = 0,
                                IsVisible = imageSource != null
                            }
                        },
                        new Label
                        {
                            Text = $"🔗 {item.Source}",
                            FontSize = 12,
                            TextColor = Color.FromArgb(item.SourceColor),
                            HorizontalOptions = LayoutOptions.Start
                        },
                        new Label
                        {
                            Text = item.Title,
                            FontSize = 18,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.White,
                            MaxLines = 6
                        },
                        new Label
                        {
                            Text = item.Content,
                            FontSize = 14,
                            TextColor = Color.FromArgb("#CCCCCC"),
                            MaxLines = 9
                        },
                        new Button
                        {
                            Text = "Читать →",
                            TextColor = Colors.White,
                            BackgroundColor = Color.FromArgb("#2D2D2D"),
                            CornerRadius = 6,
                            Padding = new Thickness(12, 6),
                            HorizontalOptions = LayoutOptions.End,
                            Command = new Command(async () => await OpenUrl(item.Url))
                        }
                    }
                }
            };

            if (DeviceInfo.Platform == DevicePlatform.WinUI || DeviceInfo.Platform == DevicePlatform.macOS)
            {
                newsCard.MaximumWidthRequest = 650;
                newsCard.HorizontalOptions = LayoutOptions.Center;
            }

            NewsContainer.Children.Add(newsCard);
        }

        private Frame CreateNoInternetCard()
        {
            var noInternetCard = new Frame
            {
                BorderColor = Colors.Transparent,
                BackgroundColor = Color.FromArgb("#2D2D2D"),
                Padding = 15,
                CornerRadius = 12,
                HasShadow = true,
                MaximumWidthRequest = (DeviceInfo.Platform == DevicePlatform.WinUI || DeviceInfo.Platform == DevicePlatform.macOS) ? 600 : double.PositiveInfinity,
                HorizontalOptions = LayoutOptions.Center,
                Content = new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        new Frame
                        {
                            CornerRadius = 10,
                            IsClippedToBounds = true,
                            Padding = 0,
                            Margin = 0,
                            BorderColor = Colors.Transparent,
                            BackgroundColor = Colors.Transparent,
                            Content = new Image
                            {
                                Source = ImageSource.FromFile("no_internet_oposs.jpg"),
                                Aspect = Aspect.AspectFill,
                                HeightRequest = 200,
                                HorizontalOptions = LayoutOptions.Fill,
                                Margin = 0
                            }
                        },
                        new Label
                        {
                            Text = "🔴 No internet",
                            FontSize = 12,
                            TextColor = Colors.Red,
                            HorizontalOptions = LayoutOptions.Start
                        },
                        new Label
                        {
                            Text = "Нет подключения к интернету",
                            FontSize = 18,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.White,
                            MaxLines = 6
                        },
                        new Label
                        {
                            Text = "Проверьте подключение и попробуйте снова.",
                            FontSize = 14,
                            TextColor = Color.FromArgb("#CCCCCC"),
                            MaxLines = 9
                        }
                    }
                }
            };

            return noInternetCard;
        }

        private ImageSource GetImageSource(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return GetFallbackImage();

                if (imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    return new UriImageSource
                    {
                        Uri = new Uri(imageUrl),
                        CachingEnabled = true,
                        CacheValidity = TimeSpan.FromDays(1)
                    };
                }

                return ImageSource.FromFile(imageUrl) ?? GetFallbackImage();
            }
            catch
            {
                return GetFallbackImage();
            }
        }

        private ImageSource GetFallbackImage()
        {
            return ImageSource.FromFile("long_black.jpg");
        }

        private async Task OpenUrl(string url)
        {
            try
            {
                await Launcher.OpenAsync(new Uri(url));
            }
            catch
            {
                await DisplayAlert("Ошибка", "Не удалось открыть статью", "OK");
            }
        }

        private void OnSwiped(object sender, SwipedEventArgs e)
        {
            if (e.Direction == SwipeDirection.Right)
            {
                OpenSideMenu();
            }
        }

        private async void OnRefreshButtonClicked(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                await RefreshNews();
            }
        }

        private async Task RefreshNews()
        {
            try
            {
                SetLoadingState(true);

                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    Dispatcher.Dispatch(() =>
                    {
                        NewsContainer.Children.Clear();
                        NewsContainer.Children.Add(CreateNoInternetCard());
                    });
                }
                else
                {
                    try
                    {
                        var newsItems = await _newsParser.ParseNewsAsync();
                        if (newsItems == null || newsItems.Count == 0)
                        {
                            Dispatcher.Dispatch(() =>
                            {
                                NewsContainer.Children.Clear();
                                NewsContainer.Children.Add(CreateNoInternetCard());
                            });
                        }
                        else
                        {
                            _allNews = newsItems;
                            ResetNewsView();
                        }
                    }
                    catch (Exception)
                    {
                        Dispatcher.Dispatch(() =>
                        {
                            NewsContainer.Children.Clear();
                            NewsContainer.Children.Add(CreateNoInternetCard());
                        });
                    }
                }

                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка обновления: {ex.Message}", "OK");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void OpenSideMenu()
        {
            if (Shell.Current != null)
                Shell.Current.FlyoutIsPresented = true;
        }

        private void SetLoadingState(bool isLoading)
        {
            _isLoading = isLoading;
            Dispatcher.Dispatch(() =>
            {
                LoadingIndicator.IsVisible = isLoading;
                LoadingIndicator.IsRunning = isLoading;
                NewsContainer.Opacity = isLoading ? 0.5 : 1;
            });
        }
    }
}