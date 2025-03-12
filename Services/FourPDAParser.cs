using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Net;
using System.Text.RegularExpressions;
using IElement = AngleSharp.Dom.IElement;

namespace TechnoPoss.Services
{
    public class FourPDAParser
    {
        private const string BaseUrl = "https://4pda.to";
        private readonly HtmlParser _htmlParser = new();

        private readonly (string Path, string[] Tags)[] _targetSections =
        {
            ("/news/", new[] { "смартфоны", "гаджеты", "техника" }),
            ("/tag/appliances/", new[] { "бытовая техника" }),
            ("/pages/", Array.Empty<string>())
        };

        public async Task<List<NewsItem>> Parse4PDANewsAsync(HttpClient httpClient)
        {
            var result = new List<NewsItem>();

            try
            {
                foreach (var section in _targetSections)
                {
                    var response = await httpClient.GetAsync($"{BaseUrl}{section.Path}");
                    if (!response.IsSuccessStatusCode) continue;

                    var html = await response.Content.ReadAsStringAsync();
                    result.AddRange(await ParseHtml(html, section.Tags));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"4PDA Parser Error: {ex}");
            }

            return result
                .DistinctBy(item => item.Url)
                .ToList();
        }

        private async Task<List<NewsItem>> ParseHtml(string html, string[] allowedTags)
        {
            var document = await _htmlParser.ParseDocumentAsync(html);
            var articles = document.QuerySelectorAll("article.post:not(.post-preview)");

            return articles
                .Where(article => HasAllowedTags(article, allowedTags))
                .Select(article => new NewsItem
                {
                    Source = "4pda.to",
                    Title = ExtractTitle(article),
                    Content = ExtractContent(article),
                    Url = ExtractUrl(article)
                }).ToList();
        }

        private bool HasAllowedTags(IElement article, string[] allowedTags)
        {
            if (allowedTags.Length == 0) return true;

            var tags = article.QuerySelectorAll("a.tag")
                .Select(t => t.TextContent.ToLower().Trim());

            return tags.Intersect(allowedTags).Any();
        }

        private string ExtractTitle(IElement article)
        {
            return article.QuerySelector("h2.post-title a")?
                .TextContent?
                .Trim() ?? "Без заголовка";
        }

        private string ExtractContent(IElement article)
        {
            var element = article.QuerySelector("div.post-content");
            return ProcessContent(element?.InnerHtml);
        }

        private string ExtractUrl(IElement article)
        {
            var path = article.QuerySelector("h2.post-title a")?
                .GetAttribute("href");

            return BuildUrl(path);
        }

        private string ProcessContent(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return "Нет содержимого";

            var cleaned = Regex.Replace(html,
                @"<script.*?</script>|<!--.*?-->|\[.*?\]|<div class=""(ad|mobile-related).*?div>",
                "",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            var text = Regex.Replace(cleaned, "<[^>]+>|&nbsp;", " ")
                .Replace("Читать полностью", "")
                .Trim();

            text = WebUtility.HtmlDecode(text);
            text = Regex.Replace(text, @"\s+", " ");

            return text.Length > 250 ? $"{text[..250]}..." : text;
        }

        private string BuildUrl(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return BaseUrl;

            return path.StartsWith("/") ? $"{BaseUrl}{path}" : path;
        }
    }
}