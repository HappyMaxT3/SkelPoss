using System.Collections.ObjectModel;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using Microsoft.Maui.Controls;

namespace SkelAppliences
{
    public partial class NewsPage : ContentPage
    {
        public ObservableCollection<NewsItem> NewsItems { get; } = new();

        public NewsPage()
        {
            InitializeComponent();
            LoadNews();

            // Добавляем жесты свайпа
            var swipeDown = new SwipeGestureRecognizer { Direction = SwipeDirection.Down };
            swipeDown.Swiped += OnSwiped;

            var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
            swipeRight.Swiped += OnSwiped;

            NewsContainer.GestureRecognizers.Add(swipeDown);
            NewsContainer.GestureRecognizers.Add(swipeRight);
        }

        private async void LoadNews()
        {
            try
            {
                SetLoadingIndicator(true);

                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var scriptPath = Path.Combine(baseDirectory, "BotScripts", "news_parser.py");

                if (!File.Exists(scriptPath))
                {
                    await DisplayAlert("Ошибка", $"Файл скрипта не найден:\n{scriptPath}", "OK");
                    return;
                }

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "python.exe",
                        Arguments = $"\"{scriptPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8,
                        WorkingDirectory = Path.GetDirectoryName(scriptPath)
                    }
                };

                process.Start();
                var jsonOutput = await process.StandardOutput.ReadToEndAsync();
                var errorOutput = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                jsonOutput = jsonOutput.Trim('\uFEFF');
                jsonOutput = WebUtility.HtmlDecode(jsonOutput);

                if (!string.IsNullOrEmpty(errorOutput))
                {
                    await DisplayAlert("Python Error", errorOutput, "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(jsonOutput))
                {
                    await DisplayAlert("Error", "No data received", "OK");
                    return;
                }

                var news = JsonConvert.DeserializeObject<List<NewsItem>>(jsonOutput)
                    ?? new List<NewsItem>();

                Dispatcher.Dispatch(() =>
                {
                    NewsContainer.Children.Clear();
                    foreach (var item in news)
                    {
                        AddNewsItem(item.Title, item.Content, item.Url);
                    }
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ошибка: {ex.Message}", "OK");
            }
            finally
            {
                SetLoadingIndicator(false);
            }
        }

        private void SetLoadingIndicator(bool isLoading)
        {
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;
        }

        private void AddNewsItem(string title, string content, string url)
        {
            var stackLayout = new StackLayout();

            stackLayout.Children.Add(new Label
            {
                Text = title,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            });

            stackLayout.Children.Add(new Label
            {
                Text = content,
                FontSize = 14,
                TextColor = Color.FromArgb("#CCCCCC"),
                Margin = new Thickness(0, 5)
            });

            var readMoreLabel = new Label
            {
                Text = "Читать статью...",
                FontSize = 14,
                TextColor = Colors.Blue,
                TextDecorations = TextDecorations.Underline,
                HorizontalOptions = LayoutOptions.End
            };

            readMoreLabel.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    try
                    {
                        await Launcher.OpenAsync(new Uri(url));
                    }
                    catch
                    {
                        await DisplayAlert("Ошибка", "Не удалось открыть статью", "OK");
                    }
                })
            });

            stackLayout.Children.Add(readMoreLabel);

            var frame = new Frame
            {
                BackgroundColor = Color.FromArgb("#2D2D2D"),
                Padding = 10,
                Margin = new Thickness(0, 5),
                CornerRadius = 10,
                Content = stackLayout
            };

            NewsContainer.Children.Add(frame);
        }
        private void OnSwiped(object? sender, SwipedEventArgs e)
        {
            switch (e.Direction)
            {
                case SwipeDirection.Down:
                    LoadNews();
                    break;

                case SwipeDirection.Right:
                    if (Shell.Current != null)
                        Shell.Current.FlyoutIsPresented = true;
                    break;
            }
        }
    }

    public class NewsItem
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}