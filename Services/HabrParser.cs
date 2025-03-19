using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Net;
using System.Text.RegularExpressions;
using IElement = AngleSharp.Dom.IElement;

namespace TechnoPoss.Services
{
    public class HabrParser
    {
        private const string BaseUrl = "https://habr.com";
        private readonly HtmlParser _htmlParser;
        private readonly Random _random = new();
        private readonly string[] _placeholderImages = {
            "long_black.jpg",
            "long_green.jpg",
            "long_orange.jpg",
            "long_pink.jpg",
            "long_violet.jpg"
        };

        private readonly string[] _targetFlows =
        {
            "ru/flows/develop/",
            "ru/news/",
            "ru/hubs/gadgets/"
        };

        public HabrParser()
        {
            _htmlParser = new HtmlParser();
        }

        public async Task<List<NewsItem>> ParseHabrNewsAsync(HttpClient httpClient)
        {
            var result = new List<NewsItem>();

            try
            {
                foreach (var flow in _targetFlows)
                {
                    var response = await httpClient.GetAsync($"{BaseUrl}/{flow}");
                    response.EnsureSuccessStatusCode();
                    var html = await response.Content.ReadAsStringAsync();
                    result.AddRange(await ParseHtml(html));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }

            return Shuffle(result.DistinctBy(item => item.Url).ToList());
        }

        private List<NewsItem> Shuffle(List<NewsItem> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (list[j], list[i]) = (list[i], list[j]);
            }
            return list;
        }

        private async Task<List<NewsItem>> ParseHtml(string html)
        {
            var document = await _htmlParser.ParseDocumentAsync(html);
            var articles = document.QuerySelectorAll("article.tm-articles-list__item, article.tm-article-snippet");

            return articles.Select(article => new NewsItem
            {
                Source = "habr.com",
                SourceColor = "#F4A261",
                Title = ExtractTitle(article),
                Content = ExtractContent(article),
                Url = ExtractUrl(article),
                ImageUrl = ExtractImage(article)
            }).ToList();
        }

        private string ExtractImage(IElement article)
        {
            var imageUrl = article.QuerySelector(".tm-article-snippet__cover img, .article-formatted-body img")?
                .GetAttribute("src")?
                .Trim();

            // Если изображение не найдено, используем локальный файл из ресурсов
            if (string.IsNullOrEmpty(imageUrl))
            {
                return _placeholderImages[_random.Next(_placeholderImages.Length)];
            }

            // Обработка относительных URL
            if (!imageUrl.StartsWith("http"))
            {
                return $"{BaseUrl}{imageUrl}";
            }

            return imageUrl;
        }

        private string ExtractTitle(IElement article)
        {
            return article.QuerySelector("h2.tm-title a, a.tm-article-snippet__title-link")?
                .TextContent?
                .Trim() ?? "No title";
        }

        private string ExtractContent(IElement article)
        {
            var element = article.QuerySelector(
                "div.tm-article-body, " +
                "div.tm-article-snippet__content, " +
                "div.article-formatted-body, " +
                "div.tm-article-snippet__lead, " +
                "div.tm-article-snippet__text");

            return ProcessContent(element?.InnerHtml);
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
                return "Not available";

            var text = Regex.Replace(html, "<[^>]+>|&nbsp;", " ")
                .Replace("Читать далее", "")
                .Trim();

            text = WebUtility.HtmlDecode(text);

            return text.Length > 250 ? text[..250] + "..." : text;
        }

        private string BuildUrl(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return BaseUrl;

            return path.StartsWith("/") ? $"{BaseUrl}{path}" : path;
        }
    }
}