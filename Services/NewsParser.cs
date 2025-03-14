using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Net;
using IElement = AngleSharp.Dom.IElement;

namespace TechnoPoss.Services
{
    public class NewsParser
    {
        private readonly HttpClient _httpClient;
        private readonly HabrParser _habrParser;
        private readonly FourPDAParser _4pdaParser;
        //private readonly TechRadarParser _techRadarParser;

        private readonly Random _random = new();

        public NewsParser()
        {
            var handler = new HttpClientHandler();

#if ANDROID || IOS
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert?.Issuer.Contains("Let's Encrypt") == true)
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
#endif

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(25)
            };

            ConfigureHttpClient();
            _habrParser = new HabrParser();
            _4pdaParser = new FourPDAParser();
            //_techRadarParser = new TechRadarParser();
        }

        private void ConfigureHttpClient()
        {
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36";
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml");
        }

        public async Task<List<NewsItem>> ParseNewsAsync()
        {
            try
            {
                //var techRadarTask = _techRadarParser.ParseTechRadarNewsAsync(_httpClient);
                var habrTask = _habrParser.ParseHabrNewsAsync(_httpClient);
                var pdaTask = _4pdaParser.Parse4PDANewsAsync(_httpClient);

                //await Task.WhenAll(habrTask, pdaTask, techRadarTask);
                await Task.WhenAll(habrTask, pdaTask);
                await Task.WhenAll(habrTask);

                var combinedNews = habrTask.Result
                    .Concat(pdaTask.Result)
                    .GroupBy(x => x.Url)
                    .Select(g => g.First())
                    .ToList();

                return ShuffleArticles(combinedNews);
            }
            catch
            {
                return new List<NewsItem>();
            }
        }

        private List<NewsItem> ShuffleArticles(List<NewsItem> articles)
        {

            int n = articles.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                (articles[k], articles[n]) = (articles[n], articles[k]);
            }
            return articles;
        }
    }

    public class NewsItem
    {
        public string Source { get; set; } = string.Empty;
        public string SourceColor {  get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}