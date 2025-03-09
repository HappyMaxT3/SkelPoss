using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Net;
using System.Text.RegularExpressions;
using IElement = AngleSharp.Dom.IElement;

namespace TechnoPoss.Services
{
    public class NewsParser
    {
        private readonly HttpClient _httpClient;
        private readonly HtmlParser _htmlParser;
        private const string BaseUrl = "https://habr.com";

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
            _htmlParser = new HtmlParser();
        }

        private void ConfigureHttpClient()
        {
            var userAgent = GetDesktopUserAgent();
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml");
        }

        private string GetDesktopUserAgent()
        {
            return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36";
        }

        public async Task<List<NewsItem>> ParseNewsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/ru/flows/develop/");
                response.EnsureSuccessStatusCode();
                var html = await response.Content.ReadAsStringAsync();
                return await ParseHtml(html);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return new List<NewsItem>();
            }
        }

        private async Task<List<NewsItem>> ParseHtml(string html)
        {
            var document = await _htmlParser.ParseDocumentAsync(html);
            var articles = document.QuerySelectorAll("article.tm-articles-list__item, article.tm-article-snippet");

            return articles.Select(article => new NewsItem
            {
                Title = ExtractTitle(article),
                Content = ExtractContent(article),
                Url = ExtractUrl(article)
            }).ToList();
        }

        private string ExtractTitle(AngleSharp.Dom.IElement article)
        {
            return article.QuerySelector("h2.tm-title a, a.tm-article-snippet__title-link")?
                .TextContent?
                .Trim() ?? "Без заголовка";
        }

        private string ExtractContent(AngleSharp.Dom.IElement article)
        {
            var element = article.QuerySelector(
                "div.tm-article-body, " +
                "div.tm-article-snippet__content, " +
                "div.article-formatted-body, " +
                "div.tm-article-snippet__lead, " +
                "div.tm-article-snippet__text");

            if (element == null)
                return "Нет содержимого";

            string contentHtml = element.InnerHtml;

            if (string.IsNullOrWhiteSpace(contentHtml))
            {
                contentHtml = element.TextContent;
            }

            return ProcessContent(contentHtml);
        }

        private string ExtractUrl(IElement article)
        {
            var path = article.QuerySelector("a.tm-article-snippet__title-link, h2.tm-title a")?
                .GetAttribute("href");

            return BuildUrl(path);
        }

        private string ProcessContent(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return "Нет содержимого";

            var text = Regex.Replace(html, "<[^>]+>|&nbsp;", " ")
                .Replace("Читать далее", "")
                .Trim();

            text = WebUtility.HtmlDecode(text);

            return text.Length > 250 ? text[..250] + "..." : text;
        }

        private string BuildUrl(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return $"{BaseUrl}/ru/flows/develop/";

            return path.StartsWith("/") ? $"{BaseUrl}{path}" : path;
        }
    }

    public class NewsItem
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
