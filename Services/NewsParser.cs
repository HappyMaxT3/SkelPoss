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
#if ANDROID
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MyNewsApp/1.0)");

            _htmlParser = new HtmlParser();
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

            return content.Length > 256 ? content[..256] + "..." : content;
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