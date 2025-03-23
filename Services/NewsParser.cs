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
        private readonly TechRadarParser _techRadarParser;

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
            _techRadarParser = new TechRadarParser();
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
                var techRadarTask = _techRadarParser.ParseTechRadarNewsAsync(_httpClient);
                var habrTask = _habrParser.ParseHabrNewsAsync(_httpClient);
                var pdaTask = _4pdaParser.Parse4PDANewsAsync(_httpClient);

                await Task.WhenAll(habrTask, pdaTask, techRadarTask);

                var habrNews = habrTask.Result;
                var pdaNews = pdaTask.Result;
                var techRadarNews = techRadarTask.Result;

                habrNews = habrNews.GroupBy(x => x.Url).Select(g => g.First()).ToList();
                pdaNews = pdaNews.GroupBy(x => x.Url).Select(g => g.First()).ToList();
                techRadarNews = techRadarNews.GroupBy(x => x.Url).Select(g => g.First()).ToList();

                habrNews = ShuffleWithinSource(habrNews);
                pdaNews = ShuffleWithinSource(pdaNews);
                techRadarNews = ShuffleWithinSource(techRadarNews);

                var combinedNews = DistributeArticlesRandomly(habrNews, pdaNews, techRadarNews);

                return combinedNews;
            }
            catch
            {
                return new List<NewsItem>();
            }
        }

        private List<NewsItem> ShuffleWithinSource(List<NewsItem> articles)
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

        private List<NewsItem> DistributeArticlesRandomly(params List<NewsItem>[] articleLists)
        {
            var result = new List<NewsItem>();
            var sources = articleLists.Select((list, index) => new { Articles = list, Index = index })
                                     .Where(x => x.Articles.Any())
                                     .ToList();

            if (!sources.Any())
                return result;

            var remainingArticles = sources.Select(x => new List<NewsItem>(x.Articles)).ToList();
            var sourceCounts = new int[sources.Count]; 

            int totalArticles = remainingArticles.Sum(list => list.Count);

            var weights = Enumerable.Repeat(1.0, sources.Count).ToList();

            // Заполняем ленту
            while (result.Count < totalArticles)
            {
                double totalWeight = weights.Sum();
                if (totalWeight <= 0)
                    break;

                double randomValue = _random.NextDouble() * totalWeight;
                int selectedSourceIndex = -1;
                for (int i = 0; i < weights.Count; i++)
                {
                    randomValue -= weights[i];
                    if (randomValue <= 0)
                    {
                        selectedSourceIndex = i;
                        break;
                    }
                }

                if (selectedSourceIndex == -1)
                    selectedSourceIndex = weights.Count - 1;

                var selectedArticles = remainingArticles[selectedSourceIndex];
                if (selectedArticles.Any())
                {
                    result.Add(selectedArticles[0]);
                    selectedArticles.RemoveAt(0);
                    sourceCounts[selectedSourceIndex]++;
                }

                for (int i = 0; i < weights.Count; i++)
                {
                    if (!remainingArticles[i].Any())
                    {
                        weights[i] = 0; 
                        continue;
                    }

                    double usageFactor = 1.0 - (sourceCounts[i] / (double)(result.Count + 1));
                    weights[i] = usageFactor * (remainingArticles[i].Count / (double)totalArticles);
                    if (weights[i] < 0.1)
                        weights[i] = 0.1;
                }
            }

            return result;
        }
    }

    public class NewsItem
    {
        public string Source { get; set; } = string.Empty;
        public string SourceColor { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}