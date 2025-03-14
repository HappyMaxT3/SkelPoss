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

        private const string DefaultImage = "opossum.jpg";

        private class Section
        {
            public string Path { get; set; }
            public string[] Tags { get; set; }

            public Section(string path, string[] tags)
            {
                Path = path;
                Tags = tags;
            }
        }

        private readonly Section[] _targetSections =
        {
            new Section("/tag/appliances/", new string[] { })
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
            var articleContainer = document.QuerySelector(".ZkeD2jtVkQFZAv49p6R");
            if (articleContainer == null)
            {
                Console.WriteLine("Контейнер статей не найден");
                return new List<NewsItem>();
            }

            var articles = articleContainer.QuerySelectorAll(".post.ufjEON");

            var newsItems = new List<NewsItem>();

            foreach (var article in articles.Where(a => HasAllowedTags(a, allowedTags)))
            {
                var title = ExtractTitle(article);
                var description = ExtractDescription(article);
                var url = ExtractUrl(article);
                var imageUrl = ExtractImageUrl(article);

                newsItems.Add(new NewsItem
                {
                    Source = "4pda.to",
                    SourceColor = "#D6E4F0",
                    Title = title,
                    Content = description,
                    Url = url,
                    ImageUrl = imageUrl
                });
            }

            return newsItems;
        }

        private string ExtractImageUrl(IElement article)
        {
            var previewContainer = article.QuerySelector(".WHalBy9w");
            if (previewContainer == null) return DefaultImage;

            var imgElement = previewContainer.QuerySelector("img");
            if (imgElement == null) return DefaultImage;

            var src = imgElement.GetAttribute("src");
            if (string.IsNullOrWhiteSpace(src)) return DefaultImage;

            return src.StartsWith("http") ? src : $"{BaseUrl}{src}";
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
            return article.QuerySelector(".list-post-title")?
                .TextContent?
                .Trim() ?? "No title";
        }

        private string ExtractDescription(IElement article)
        {
            var descriptionContainer = article.QuerySelector(".description");
            if (descriptionContainer == null)
            {
                Console.WriteLine("Контейнер .description не найден");
                return "Not available";
            }

            var itempropDescription = descriptionContainer.QuerySelector("[itemprop='description']");
            if (itempropDescription == null)
            {
                Console.WriteLine("Элемент [itemprop='description'] не найден внутри .description");
                return "Not available";
            }

            var paragraphs = itempropDescription.QuerySelectorAll("p");
            if (paragraphs == null || !paragraphs.Any())
            {
                Console.WriteLine("Теги <p> внутри [itemprop='description'] не найдены");
                return "Not available";
            }

            var text = string.Join(" ", paragraphs.Select(p => p.TextContent?.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)));
            if (string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine("Текст в <p> пустой или отсутствует");
                return "Not available";
            }

            text = WebUtility.HtmlDecode(text);
            text = Regex.Replace(text, @"\s+", " ");
            return text.Length > 250 ? $"{text[..250]}..." : text;
        }

        private string ExtractUrl(IElement article)
        {
            var path = article.QuerySelector(".list-post-title a")?
                .GetAttribute("href");

            return BuildUrl(path);
        }

        private string BuildUrl(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return BaseUrl;

            return path.StartsWith("/") ? $"{BaseUrl}{path}" : path;
        }
    }
}