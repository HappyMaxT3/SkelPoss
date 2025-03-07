using System.Collections.ObjectModel;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using Microsoft.Maui.Controls;
using AngleSharp.Html.Parser;
using System.Net.Http;

namespace SkelAppliences
{
    public partial class NewsPage : ContentPage
    {
        private const int PageSize = 5;
        private int _currentPage = 0;
        private List<NewsItem> _allNews = new();
        private bool _isLoadingMore;
        private bool _isInitialLoading = true;
        private HttpClient _httpClient;

        public NewsPage()
        {
            InitializeComponent();
            InitializeHttpClient();
            SetupScrollListener();
            LoadInitialNews();
        }

        private void InitializeHttpClient()
        {
            var handler = new HttpClientHandler();
#if ANDROID
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif
            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MyNewsApp/1.0)");
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        private void SetupScrollListener()
        {
            MainScroll.Scrolled += async (sender, e) =>
            {
                if (_isInitialLoading || _isLoadingMore) return;

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
                _allNews = await ParseNews();
                DisplayNewsPage();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
            finally
            {
                SetLoadingIndicator(false);
                _isInitialLoading = false;
            }
        }

        private async Task<List<NewsItem>> ParseNews()
        {
            var news = new List<NewsItem>();

            try
            {
                var response = await _httpClient.GetAsync("https://habr.com/ru/flows/develop/");
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync();
                var parser = new HtmlParser();
                var document = await parser.ParseDocumentAsync(html);

                foreach (var article in document.QuerySelectorAll("article.tm-articles-list__item"))
                {
                    var titleElem = article.QuerySelector("h2.tm-title a");
                    var contentElem = article.QuerySelector("div.tm-article-body");
                    var linkElem = titleElem?.GetAttribute("href");

                    var title = titleElem?.TextContent.Trim() ?? "Без заголовка";
                    var content = contentElem?.TextContent.Trim() ?? "Нет содержимого";
                    var url = linkElem != null ? new Uri(new Uri("https://habr.com"), linkElem).ToString() : "#";

                    news.Add(new NewsItem
                    {
                        Title = title,
                        Content = content.Length > 256 ? content[..256] + "..." : content,
                        Url = url
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка парсинга: {ex}");
                throw;
            }

            return news;
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
                _allNews = await ParseNews();
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

    public class NewsItem
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}