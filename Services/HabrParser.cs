﻿using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Net;
using System.Text.RegularExpressions;

namespace TechnoPoss.Services
{
    public class HabrParser
    {
        private const string BaseUrl = "https://habr.com";
        private readonly HtmlParser _htmlParser;

        public HabrParser()
        {
            _htmlParser = new HtmlParser();
        }

        public async Task<List<NewsItem>> ParseHabrNewsAsync(HttpClient httpClient)
        {
            try
            {
                var response = await httpClient.GetAsync($"{BaseUrl}/ru/flows/develop/");
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

            return ProcessContent(element?.InnerHtml);
        }

        private string ExtractUrl(AngleSharp.Dom.IElement article)
        {
            var path = article.QuerySelector("a.tm-article-snippet__title-link, h2.tm-title a")?
                .GetAttribute("href");

            return BuildUrl(path);
        }

        private string ProcessContent(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return "Нет содержимого";

            var text = Regex.Replace(html, "<[^>]+>|&nbsp;", " ")
                .Replace("Читать далее", "")
                .Trim();

            text = WebUtility.HtmlDecode(text);

            return text.Length > 250 ? text[..250] + "..." : text;
        }

        private string BuildUrl(string path)
        {
            if (string.IsNullOrEmpty(path))
                return $"{BaseUrl}/ru/flows/develop/";

            return path.StartsWith("/") ? $"{BaseUrl}{path}" : path;
        }
    }
}