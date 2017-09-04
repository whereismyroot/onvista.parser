using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Threading;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Onvista.Parser.Logging;

namespace Onvista.Parser
{
    public class NewsParser
    {
        private const string PageQueryStringKey = "page";
        private const string RootPath = "https://www.onvista.de";
        private readonly string[] _analysisValues = { "StrongBuy", "Buy", "Hold", "Underperform", "Sell" };
        private readonly int _requestDelayMs;

        private HtmlParser _htmlParser = new HtmlParser();
        private WebClient _webClient = new WebClient { Encoding = System.Text.Encoding.UTF8 };
        private ILogger _logger;
        
        public NewsParser(int requestDelayMs)
        {
            _requestDelayMs = requestDelayMs;
            _logger = new ParserLogger();
        }

        public List<Article> ParseNews(string url, int? pagesToParse)
        {
            List<Article> resultArticles = new List<Article>();

            try
            {
                IHtmlDocument firstPage = GetDocumentFromUrl(url);

                int pagesCount = GetPagesCount(firstPage);
                int pagesCountToParse = pagesToParse.HasValue ? Math.Min(pagesCount, pagesToParse.Value) : pagesCount;

                _logger.LogInformation($"Parsing of {url} was started. Pages to parse: {pagesCountToParse}");

                resultArticles.AddRange(GetArticlesFromDocument(firstPage));

                for (int page = 2; page < pagesCountToParse; page++)
                {
                    string pageUrl = GetPageUrl(url, page);
                    var document = GetDocumentFromUrl(pageUrl);
                    var articles = GetArticlesFromDocument(document);

                    resultArticles.AddRange(articles);

                    _logger.LogInformation($"Page {page} was parsed. Articles count: {resultArticles.Count}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while parsing news: {resultArticles.Count} articles were parsed", ex);
            }

            return resultArticles;
        }

        private List<Article> GetArticlesFromDocument(IHtmlDocument document)
        {
            List<Article> resultArticles = new List<Article>();

            try
            {
                foreach (string url in GetArticleUrlsFromDocument())
                {
                    IHtmlDocument articleDocument = GetDocumentFromUrl(url);
                    Article article = GetArticleFromDocument(articleDocument);
                    resultArticles.Add(article);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected exception when parsing articles", ex);
            }

            return resultArticles;

            IEnumerable<string> GetArticleUrlsFromDocument()
            {
                IHtmlCollection<IElement> anchors = document.QuerySelectorAll("article[id] .headline-medium a");
                foreach (var element in anchors)
                {
                    yield return $"{RootPath}{element.GetAttribute("href")}";
                }
            }

            Article GetArticleFromDocument(IHtmlDocument articleDocument)
            {
                var analysisElement = articleDocument.QuerySelector(".ARTIKEL>article>.analysis");
                var headlineElement = articleDocument.QuerySelector(".ARTIKEL>article>.headline-large");
                var timeElement = articleDocument.QuerySelector(".ARTIKEL>article>cite>time");
                var authorElement = articleDocument.QuerySelector(".ARTIKEL>article>cite>span");
                var bodyElement = articleDocument.QuerySelector(".ARTIKEL>article>div>div[property=\"schema:articleBody\"]");

                string analysis = analysisElement.ClassList.Intersect(_analysisValues).FirstOrDefault();
                string headline = headlineElement.TextContent;
                string time = timeElement.TextContent;
                string author = authorElement.TextContent;
                string body = bodyElement.TextContent;

                return new Article(analysis, headline, time, author, body);
            }
        }

        private IHtmlDocument GetDocumentFromUrl(string url)
        {
            if (_requestDelayMs > 0)
            {
                Thread.Sleep(_requestDelayMs);
            }

            var content = _webClient.DownloadString(url);
            return _htmlParser.Parse(content);
        }

        private int GetPagesCount(IHtmlDocument document)
        {
            IElement lastPageAnchor = document.QuerySelector(".BLAETTER_NAVI ul li:nth-last-child(1) a");
            QueryString queryString = new QueryString(lastPageAnchor.GetAttribute("href"));
            return int.Parse(queryString[PageQueryStringKey]);
        }

        private string GetPageUrl(string url, int page)
        {
            QueryString queryString = new QueryString(url);
            queryString.Set(PageQueryStringKey, page.ToString());

            int queryBeginIndex = url.IndexOf("?");

            string resultUrl = queryBeginIndex == -1 ? $"{url}{queryString}" : $"{url.Substring(0, queryBeginIndex)}{queryString}";
            return resultUrl;
        }
    }
}
