using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Linq;
using System.Threading;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Onvista.Parser.Data;
using Onvista.Parser.Logging;

namespace Onvista.Parser
{
    public class ArticlesParser
    {
        private const string ArticleDateFormat = "dd.MM.yy, HH:mm";
        private const string MySqlDateFormat = "yyyy.MM.dd HH:mm";
        private const string PageQueryStringKey = "page";
        private const string RootPath = "https://www.onvista.de";

        private readonly string[] _analysisValues = { "StrongBuy", "Buy", "Hold", "Underperform", "Sell" };
        private readonly IArticlesRepository _articlesRepository;
        private readonly HashSet<string> _existingArticles = new HashSet<string>();

        private readonly HtmlParser _htmlParser = new HtmlParser();
        private readonly WebClient _webClient = new WebClient { Encoding = System.Text.Encoding.UTF8 };
        private readonly ILogger _logger;

        public ArticlesParser(IArticlesRepository articlesRepository)
        {
            _articlesRepository = articlesRepository;
            _logger = new ParserLogger();
        }

        public List<ParsingResult<Article>> ParseArticles(OnvistaArticlesParsingParameters parameters)
        {
            string url = parameters.NewsUrl;
            int? pagesToParse = parameters.PagesToParse;
            bool stopParsingOnExistingRecord = parameters.StopParsingOnExisting;
            int skipPages = parameters.SkipPages;
            int requestDelay = parameters.RequestDelayMs;

            List<ParsingResult<Article>> resultArticles = new List<ParsingResult<Article>>();

            RefreshExistingArticles();

            try
            {
                int pagesCount = -1;
                int pagesCountToParse = 1;

                for (int page = 1; page <= pagesCountToParse; page++)
                {
                    if (pagesCount != -1 && skipPages > 0)
                    {
                        skipPages--;
                        continue;
                    }

                    string pageUrl = GetPageUrl(url, page);
                    var document = GetDocumentFromUrl(pageUrl, requestDelay);

                    if (pagesCount == -1)
                    {
                        pagesCount = GetPagesCount(document);
                        pagesCountToParse = pagesToParse.HasValue ? Math.Min(pagesCount, pagesToParse.Value) : pagesCount;
                    }

                    if (skipPages > 0)
                    {
                        skipPages--;
                        continue;
                    }

                    _logger.LogInformation($"Parsing of page {page}/{pagesCountToParse} started.");

                    var articles = GetArticlesFromDocument(document, requestDelay);

                    resultArticles.AddRange(articles);

                    _logger.LogInformation($"Page {page} was parsed. Articles count: {resultArticles.Count}");

                    var articlesToSave = articles.Where(x => x.ResultType == ParsingResultType.PendingForSave).ToList();

                    if (articlesToSave.Any() && parameters.SaveWithParsing)
                    {
                        _logger.LogInformation("Saving...");
                        SaveParsingResults(articlesToSave);
                    }

                    if (stopParsingOnExistingRecord &&
                        articles.Any(x => x.ResultType == ParsingResultType.AlreadyExists))
                    {
                        _logger.LogInformation("Stopping parser: already existing records were parsed");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while parsing articles: {resultArticles.Count} articles were parsed", ex);
            }

            return resultArticles;
        }

        public bool SaveParsingResults(List<ParsingResult<Article>> parsingResults)
        {
            bool success = false;

            try
            {
                foreach (var parsingResult in parsingResults)
                {
                    parsingResult.Entity.CreatedAt = DateTime.ParseExact(parsingResult.Entity.CreatedAt, ArticleDateFormat, CultureInfo.InvariantCulture)
                        .ToString(MySqlDateFormat);

                    _articlesRepository.Insert(parsingResult.Entity);

                    parsingResult.ResultType = ParsingResultType.Saved;
                }

                success = true;
                _logger.LogInformation($"{parsingResults.Count} records were saved");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while saving parser records", ex);
            }

            return success;
        }

        private void RefreshExistingArticles()
        {
            _existingArticles.Clear();

            try
            {
                var existing = _articlesRepository.GetEntities(string.Empty);

                foreach (var article in existing)
                {
                    _existingArticles.Add(article.RelativeUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception, while fetching articles", ex);
                throw;
            }
        }

        private List<ParsingResult<Article>> GetArticlesFromDocument(IHtmlDocument document, int requestDelay)
        {
            List<ParsingResult<Article>> resultArticles = new List<ParsingResult<Article>>();

            try
            {
                foreach (string url in GetArticleUrlsFromDocument())
                {
                    IHtmlDocument articleDocument = GetDocumentFromUrl(url, requestDelay);

                    Article article = GetArticleFromDocument(url, articleDocument);

                    ParsingResultType type = ParsingResultType.AlreadyExists;
                    if (!_existingArticles.Contains(article.RelativeUrl))
                    {
                        _existingArticles.Add(article.RelativeUrl);
                        type = ParsingResultType.PendingForSave;
                    }
                    else
                    {
                        //debug
                    }

                    resultArticles.Add(new ParsingResult<Article>(article, type));
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

            Article GetArticleFromDocument(string documentUrl, IHtmlDocument articleDocument)
            {
                var analysisElement = articleDocument.QuerySelector(".ARTIKEL>article>.analysis");
                var headlineElement = articleDocument.QuerySelector(".ARTIKEL>article>.headline-large");
                var timeElement = articleDocument.QuerySelector(".ARTIKEL>article>cite>time");
                var authorElement = articleDocument.QuerySelector(".ARTIKEL>article>cite>span");
                var bodyElement = articleDocument.QuerySelector(".ARTIKEL>article>div>div[property=\"schema:articleBody\"]");
                string relativeUrl = new Uri(documentUrl).LocalPath;

                string analysis = analysisElement.ClassList.Intersect(_analysisValues).FirstOrDefault();
                string headline = headlineElement.TextContent;
                string time = timeElement.TextContent;
                string author = authorElement.TextContent;
                string body = bodyElement.TextContent;

                return new Article(analysis?.Trim(), headline.Trim(), relativeUrl, time.Trim(), author.Trim(), body.Trim());
            }
        }

        private IHtmlDocument GetDocumentFromUrl(string url, int requestDelayMs)
        {
            if (requestDelayMs > 0)
            {
                Thread.Sleep(requestDelayMs);
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
