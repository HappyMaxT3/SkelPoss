using System.Collections.ObjectModel;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Runtime.Versioning;

namespace SkelAppliences
{
    public partial class NewsPage : ContentPage
    {
        public ObservableCollection<NewsItem> NewsItems { get; } = new ObservableCollection<NewsItem>();

        public NewsPage()
        {
            InitializeComponent();
            LoadNews();
        }

        private async void LoadNews()
        {
            try
            {
                // Показываем индикатор загрузки (только если поддерживается)
                SetLoadingIndicator(true);

                var pythonPath = "python.exe";
                var scriptPath = @"C:\\Users\\user\\Desktop\\ucheba\\2_kurs\\SkelAppliances\\BotScripts\\news_parser.py";

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = pythonPath,
                        Arguments = scriptPath,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var jsonOutput = await process.StandardOutput.ReadToEndAsync();
                File.WriteAllText("debug_output.json", jsonOutput);
                process.WaitForExit();

                Console.WriteLine(jsonOutput);

                if (string.IsNullOrWhiteSpace(jsonOutput))
                {
                    await DisplayAlert("Ошибка", "Сервер не вернул данные.", "OK");
                    return;
                }

                // Десериализация JSON в список новостей
                var news = JsonConvert.DeserializeObject<List<NewsItem>>(jsonOutput);

                Dispatcher.Dispatch(() =>
                {
                    NewsItems.Clear();
                    foreach (var item in news)
                    {
                        AddNewsItem(item.Title, item.Content);
                    }
                });

            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить новости: {ex.Message}", "OK");
            }
            finally
            {
                // Скрываем индикатор загрузки (только если поддерживается)
                SetLoadingIndicator(false);
            }
        }

        [SupportedOSPlatform("android21.0")]
        private void SetLoadingIndicator(bool isLoading)
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(21))
            {
                LoadingIndicator.IsVisible = isLoading;
                LoadingIndicator.IsRunning = isLoading;
            }
        }

        private void AddNewsItem(string title, string content)
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
                            Text = title,
                            FontSize = 18,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.White
                        },
                        new Label
                        {
                            Text = content,
                            FontSize = 14,
                            TextColor = Color.FromArgb("#CCCCCC"),
                            Margin = new Thickness(0, 5)
                        }
                    }
                }
            };

            NewsContainer.Children.Add(frame);
        }

        private void OnSwiped(object sender, SwipedEventArgs e)
        {
            LoadNews();
        }
    }

    public class NewsItem
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
    }
}