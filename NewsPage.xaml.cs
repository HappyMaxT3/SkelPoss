using Microsoft.Maui.Controls;
using SkelAppliences.Services;
using System.Collections.ObjectModel;

namespace SkelAppliences
{
    public partial class NewsPage : ContentPage
    {
        private const int PageSize = 4;
        private int _currentPage = 0;
        private List<NewsItem> _allNews = new();
        private bool _isLoadingMore;
        private readonly NewsParser _newsParser = new();

        public NewsPage()
        {
            InitializeComponent();
            SetupScrollListener();
            LoadInitialNews();
        }

        private void SetupScrollListener()
        {
            MainScroll.Scrolled += async (sender, e) =>
            {
                if (_isLoadingMore) return;

                var scrollView = (ScrollView)sender;
                var scrollingSpace = scrollView.ContentSize.Height - scrollView.Height;

                if (scrollingSpace <= scrollView.ScrollY + 100)
                {
                    _isLoadingMore = true;
                    await Task.Delay(300);
                    DisplayNewsPage();
                    _isLoadingMore = false;
                }
            };
        }

        private async void LoadInitialNews()
        {
            try
            {
                SetLoadingIndicator(true);
                _allNews = await _newsParser.ParseNewsAsync();
                DisplayNewsPage();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
            finally
            {
                SetLoadingIndicator(false);
            }
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
            var frame = new Frame
            {
                BackgroundColor = Color.FromArgb("#2D2D2D"),
                Padding = 10,
                Margin = new Thickness(0, 5),
                CornerRadius = 10,
                Content = new StackLayout
                {
                    Children =
                    {
                        new Label
                        {
                            Text = item.Title,
                            FontSize = 18,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.White
                        },
                        new Label
                        {
                            Text = item.Content,
                            FontSize = 14,
                            TextColor = Color.FromArgb("#CCCCCC"),
                            Margin = new Thickness(0, 5)
                        },
                        CreateReadMoreButton(item.Url)
                    }
                }
            };

            NewsContainer.Children.Add(frame);
        }

        private View CreateReadMoreButton(string url)
        {
            return new Button
            {
                Text = "Читать далее →",
                TextColor = Colors.White,
                BackgroundColor = Color.FromArgb("#4A90E2"),
                CornerRadius = 5,
                Margin = new Thickness(0, 5),
                HorizontalOptions = LayoutOptions.End,
                Command = new Command(async () => await OpenUrl(url))
            };
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

        private void SetLoadingIndicator(bool isLoading)
        {
            Dispatcher.Dispatch(() =>
            {
                LoadingIndicator.IsVisible = isLoading;
                LoadingIndicator.IsRunning = isLoading;
            });
        }

        private void OnSwiped(object? sender, SwipedEventArgs e)
        {
            switch (e.Direction)
            {
                case SwipeDirection.Down:
                    RefreshNews();
                    break;
                case SwipeDirection.Right:
                    OpenSideMenu();
                    break;
            }
        }

        private async void RefreshNews()
        {
            try
            {
                _currentPage = 0;
                _allNews.Clear();
                NewsContainer.Children.Clear();
                _allNews = await _newsParser.ParseNewsAsync();
                DisplayNewsPage();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        private void OpenSideMenu()
        {
            if (Shell.Current != null)
                Shell.Current.FlyoutIsPresented = true;
        }
    }
}