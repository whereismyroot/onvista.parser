using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Onvista.Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            OnvistaNewsParserParameters parserParameters = new OnvistaNewsParserParameters(args);

            NewsParser parser = new NewsParser(parserParameters.RequestDelayMs);
            var newsArticles = parser.ParseNews(parserParameters.NewsUrl, parserParameters.PagesToParse);

            string csv = new CsvWrapper(newsArticles).GetCsv();
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"articles-{DateTime.Now:dd.MM.yy HH.mm}.csv");

            File.WriteAllText(filePath, csv);

            Console.WriteLine($"Results were written to {filePath}");
        }
    }

    class OnvistaNewsParserParameters
    {
        private const int DefaultRequestDelay = 100;
        private const string DefaultNewsUrl = "https://www.onvista.de/news/alle-news?newsType[]=analysis";

        public OnvistaNewsParserParameters(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                RequestDelayMs = DefaultRequestDelay;
                NewsUrl = DefaultNewsUrl;
            }
            else
            {
                var dictionary = ParseArgs(args).ToDictionary(x => x.Key.ToLower(), x => x.Value);

                PagesToParse = dictionary.TryGetValue(nameof(PagesToParse).ToLower(), out string pagesToParse) ? (int?)int.Parse(pagesToParse) : null;
                RequestDelayMs = dictionary.TryGetValue(nameof(RequestDelayMs).ToLower(), out string requestDelayMs) ? int.Parse(requestDelayMs) : DefaultRequestDelay;
                NewsUrl = dictionary.TryGetValue(nameof(NewsUrl).ToLower(), out string newsUrl) ? newsUrl : DefaultNewsUrl;
            }

        }

        public int? PagesToParse { get; set; }

        public int RequestDelayMs { get; set; }

        public string NewsUrl { get; set; }

        private IEnumerable<KeyValuePair<string, string>> ParseArgs(string[] args)
        {
            for (int i = 0; i < args.Length - 1; i = i + 2)
            {
                int valueIndex = i + 1;
                if (valueIndex >= args.Length)
                {
                    yield break;
                }

                yield return new KeyValuePair<string, string>(args[i].Trim(), args[i + 1].Trim());
            }
        }
    }
}
