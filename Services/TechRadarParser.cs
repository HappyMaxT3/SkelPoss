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
        private readonly HtmlParser _htmlParser;
        private readonly Random _random = new();
        private readonly string[] _placeholderImages = {
            "long_black.jpg",
            "long_green.jpg",
            "long_orange.jpg",
            "long_pink.jpg",
            "long_violet.jpg"
        };

        private readonly string[] _targetSections =
        {
            "/news"
        };

        public TechRadarParser()
        {
            _htmlParser = new HtmlParser();
        }

        public async Task<List<NewsItem>> ParseTechRadarNewsAsync(HttpClient httpClient)
        {
            var result = new List<NewsItem>();

            try
            {
                foreach (var section in _targetSections)
                {
                    var response = await httpClient.GetAsync($"{BaseUrl}{section}");
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
            var newsContainer = document.QuerySelector(".listingResults.news");
            if (newsContainer == null)
            {
                return new List<NewsItem>();
            }

            var articles = newsContainer.QuerySelectorAll("[class^='listingResult small result']");

            return articles.Select(article => new NewsItem
            {
                Source = "techradar.com",
                SourceColor = "#00A1D6",  
                Title = ExtractTitle(article),
                Content = ExtractContent(article),
                Url = ExtractUrl(article),
                ImageUrl = ExtractImage(article)
            }).ToList();
        }

        private string ExtractImage(IElement article)
        {
            var imageUrl = article.QuerySelector("img")?
                .GetAttribute("src")?
                .Trim();

            if (string.IsNullOrEmpty(imageUrl))
            {
                return _placeholderImages[_random.Next(_placeholderImages.Length)];
            }

            if (!imageUrl.StartsWith("http"))
            {
                return $"{BaseUrl}{imageUrl}";
            }

            return imageUrl;
        }

        private string ExtractTitle(IElement article)
        {
            var articleLink = article.QuerySelector("a.article-link");
            var title = articleLink?.GetAttribute("aria-label")?.Trim();

            return string.IsNullOrEmpty(title) ? "No title" : title;
        }

        private string ExtractContent(IElement article)
        {
            var searchResult = article.QuerySelector("article.search-result.search-result-news.has-rating");
            if (searchResult == null)
                return "Not available";

            var paragraphs = searchResult.QuerySelectorAll("p");
            if (paragraphs == null || paragraphs.Length < 2)
            {
                var synopsis = searchResult.QuerySelector(".synopsis, .description");
                return ProcessContent(synopsis?.InnerHtml);
            }

            var descriptionElement = paragraphs.Skip(1).FirstOrDefault();
            return ProcessContent(descriptionElement?.InnerHtml);
        }

        private string ExtractUrl(IElement article)
        {
            var path = article.QuerySelector("a.article-link")?
                .GetAttribute("href");

            return BuildUrl(path);
        }

        private string ProcessContent(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return "Not available";

            var text = Regex.Replace(html, "<[^>]+>| ", " ")
                .Replace("Read more", "")
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