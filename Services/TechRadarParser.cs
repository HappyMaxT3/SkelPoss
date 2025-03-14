using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Net;
using System.Text.RegularExpressions;
using IElement = AngleSharp.Dom.IElement;

namespace TechnoPoss.Services
{
    public class TechRadarParser
    {
        private const string BaseUrl = "https://www.techradar.com";
        private readonly HtmlParser _htmlParser = new();

        public async Task<List<NewsItem>> ParseTechRadarNewsAsync(HttpClient httpClient)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.google.com/");

                var response = await httpClient.GetAsync($"{BaseUrl}/home/small-appliances/news");
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync();
                return await ParseHtml(html);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TechRadar Parser Error: {ex}");
                return new List<NewsItem>();
            }
        }

        private async Task<List<NewsItem>> ParseHtml(string html)
        {
            var document = await _htmlParser.ParseDocumentAsync(html);
            var articles = document.QuerySelectorAll("article");

            return articles.Select(article =>
            {
                try
                {
                    return new NewsItem
                    {
                        Source = "TechRadar.com",
                        SourceColor = "Red",
                        Title = ExtractTitle(article),
                        Content = ExtractContent(article),
                        Url = ExtractUrl(article),
                        ImageUrl = "img"
                    };
                }
                catch
                {
                    return null;
                }
            })
            .Where(item => item != null)
            .ToList();
        }

        private string ExtractTitle(IElement article)
        {
            return article.QuerySelector("h2 a")?
                .TextContent?
                .Trim() ?? "No title";
        }

        private string ExtractContent(IElement article)
        {
            var content = article.QuerySelector("div.article-excerpt p")?.TextContent
                        ?? article.QuerySelector(".description")?.TextContent
                        ?? string.Empty;

            return CleanContent(content);
        }

        private string ExtractUrl(IElement article)
        {
            var path = article.QuerySelector("h2 a")?
                .GetAttribute("href");

            // Обработка полных URL в некоторых статьях
            if (path?.StartsWith("http") == true)
                return path;

            return !string.IsNullOrEmpty(path)
                ? $"{BaseUrl}{path}"
                : BaseUrl;
        }

        private string CleanContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "No content available";

            // Удаляем специальные символы и HTML-теги
            var cleaned = Regex.Replace(content,
                @"\u201c|\u201d|\u2018|\u2019|<[^>]+>|&nbsp;",
                " ")
                .Replace("Read more", "")
                .Trim();

            return WebUtility.HtmlDecode(cleaned);
        }
    }
}