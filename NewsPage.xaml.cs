using System.Collections.ObjectModel;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text;
using System.Net;

namespace SkelAppliences
{
    public partial class NewsPage : ContentPage
    {
        private const int PageSize = 5;
        private int _currentPage = 0;
        private List<NewsItem> _allNews = new();
        private bool _isLoadingMore;
        private bool _isInitialLoading = true;

        public NewsPage()
        {
            InitializeComponent();
            SetupGestures();
            LoadInitialNews();
        }

        private void SetupGestures()
        {
            var swipeDown = new SwipeGestureRecognizer { Direction = SwipeDirection.Down };
            swipeDown.Swiped += OnSwiped;
            var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
            swipeRight.Swiped += OnSwiped;
            NewsContainer.GestureRecognizers.Add(swipeDown);
            NewsContainer.GestureRecognizers.Add(swipeRight);

            MainScroll.Scrolled += OnScroll;
        }

        private async void LoadInitialNews()
        {
            try
            {
                SetLoadingIndicator(true);
                await LoadNewsData();
                DisplayNewsPage();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ошибка: {ex.Message}", "OK");
            }
            finally
            {
                SetLoadingIndicator(false);
                _isInitialLoading = false;
            }
        }

        private async Task LoadNewsData()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var scriptPath = Path.Combine(baseDir, "BotScripts", "news_parser.py");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python.exe",
                    Arguments = $"\"{scriptPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    StandardOutputEncoding = Encoding.UTF8,
                }
            };

            process.Start();
            var jsonOutput = await process.StandardOutput.ReadToEndAsync();
            var errorOutput = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrEmpty(errorOutput))
                throw new Exception($"Python error: {errorOutput}");

            _allNews = JsonConvert.DeserializeObject<List<NewsItem>>(jsonOutput) ?? new();
        }

        private void DisplayNewsPage()
        {
            var start = _currentPage * PageSize;
            var end = Math.Min(start + PageSize, _allNews.Count);

            for (var i = start; i < end; i++)
            {
                AddNewsItem(_allNews[i]);
            }
            _currentPage++;
        }

        private async void OnScroll(object sender, ScrolledEventArgs e)
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
                Text = "Читать далее",
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
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;
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
            _currentPage = 0;
            _allNews.Clear();
            NewsContainer.Children.Clear();
            await LoadNewsData();
            DisplayNewsPage();
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