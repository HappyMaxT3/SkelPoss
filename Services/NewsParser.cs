using AngleSharp.Html.Parser;
using System.Net;

namespace SkelAppliences.Services
{
    public class NewsParser
    {
        private readonly HttpClient _httpClient;
        private readonly HtmlParser _htmlParser;

        public NewsParser()
        {
            var handler = new HttpClientHandler();

#if ANDROID || IOS || MACCATALYST
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            var userAgent = GetPlatformUserAgent();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

            _htmlParser = new HtmlParser();
        }

        private string GetPlatformUserAgent()
        {
#if ANDROID
            return "Mozilla/5.0 (Linux; Android 13) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.6099.210 Mobile Safari/537.36";
#elif IOS
            return "Mozilla/5.0 (iPhone; CPU iPhone OS 17_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.2 Mobile/15E148 Safari/604.1";
#elif MACCATALYST
            return "Mozilla/5.0 (Macintosh; Intel Mac OS X 14_3) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.2 Safari/605.1.15";
#elif WINDOWS
            return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
#else
            return "Mozilla/5.0 (compatible; MyNewsApp/1.0)";
#endif
        }

        public async Task<List<NewsItem>> ParseNewsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://habr.com/ru/flows/develop/");
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync();
                return await ParseHtml(html);
            }
            catch
            {
                return new List<NewsItem>();
            }
        }

        private async Task<List<NewsItem>> ParseHtml(string html)
        {
            var news = new List<NewsItem>();
            var document = await _htmlParser.ParseDocumentAsync(html);

            foreach (var article in document.QuerySelectorAll("article.tm-articles-list__item"))
            {
                var titleElem = article.QuerySelector("h2.tm-title a");
                var contentElem = article.QuerySelector("div.tm-article-body");
                var linkElem = titleElem?.GetAttribute("href");

                news.Add(new NewsItem
                {
                    Title = titleElem?.TextContent.Trim() ?? "Без заголовка",
                    Content = ProcessContent(contentElem?.TextContent.Trim()),
                    Url = ProcessUrl(linkElem)
                });
            }

            return news;
        }

        private string ProcessContent(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "Нет содержимого";

            return content.Length > 300 ? content[..300] + "..." : content;
        }

        private string ProcessUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "#";

            return new Uri(new Uri("https://habr.com"), url).ToString();
        }
    }

    public class NewsItem
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}