using AngleSharp.Html.Parser;
using System.Net;
using System.Text.RegularExpressions;

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
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => 
            {
                if (cert?.Issuer.Contains("Let's Encrypt") == true) return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
#endif

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(20)
            };

            var userAgent = GetPlatformUserAgent();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            _htmlParser = new HtmlParser(new HtmlParserOptions
            {
                IsScripting = false,
                IsEmbedded = true
            });
        }

        private string GetPlatformUserAgent()
        {
#if ANDROID
            return "Mozilla/5.0 (Linux; Android 13) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.6099.210 Mobile Safari/537.36";
#elif IOS
            return "Mozilla/5.0 (iPhone; CPU iPhone OS 17_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.2 Mobile/15E148 Safari/604.1";
#else
            return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
#endif
        }

        public async Task<List<NewsItem>> ParseNewsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://habr.com/ru/flows/develop/");
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync();

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"Received HTML: {html[..2000]}...");
#endif

                return await ParseHtml(html);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Parse error: {ex}");
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

                var contentElem = article.QuerySelector("div.tm-article-body") ??
                                article.QuerySelector("div.tm-article-snippet__content") ??
                                article.QuerySelector("div.article-formatted-body") ??
                                article.QuerySelector("div.tm-article-snippet__lead");

                var linkElem = titleElem?.GetAttribute("href");
                var content = ProcessContent(contentElem?.InnerHtml);

                news.Add(new NewsItem
                {
                    Title = CleanText(titleElem?.TextContent) ?? "Без заголовка",
                    Content = content,
                    Url = ProcessUrl(linkElem)
                });
            }

            return news;
        }

        private string ProcessContent(string? htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
                return "Нет содержимого";

            var textContent = Regex.Replace(htmlContent, "<[^>]*>", "");
            textContent = WebUtility.HtmlDecode(textContent);

            textContent = CleanText(textContent);

            return textContent.Length > 300 ?
                textContent[..300] + "..." :
                textContent;
        }

        private string CleanText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return Regex.Replace(text, @"[\r\n\t]+|\s{2,}", " ")
                      .Trim();
        }

        private string ProcessUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "#";

            try
            {
                return new Uri(new Uri("https://habr.com"), url).AbsoluteUri;
            }
            catch
            {
                return "https://habr.com";
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