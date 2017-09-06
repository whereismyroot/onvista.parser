using System;
using System.Collections.Generic;
using System.Linq;

namespace Onvista.Parser
{
    public class OnvistaArticlesParsingParameters
    {
        private const int DefaultRequestDelay = 100;
        private const string DefaultNewsUrl = "https://www.onvista.de/news/alle-news?newsType[]=analysis";

        public OnvistaArticlesParsingParameters(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                RequestDelayMs = DefaultRequestDelay;
                NewsUrl = DefaultNewsUrl;
                StopParsingOnExisting = true;
                SaveWithParsing = true;
            }
            else
            {
                var dictionary = ParseArgs(args).ToDictionary(x => x.Key.ToLower(), x => x.Value);

                PagesToParse = dictionary.TryGetValue(nameof(PagesToParse).ToLower(), out string pagesToParseString) 
                    && int.TryParse(pagesToParseString, out int pagesToParse) 
                    ? (int?)pagesToParse : null;

                RequestDelayMs = dictionary.TryGetValue(nameof(RequestDelayMs).ToLower(), out string requestDelayMsString) 
                    && int.TryParse(requestDelayMsString, out int requestDelayMs) 
                    ? requestDelayMs : DefaultRequestDelay;

                NewsUrl = dictionary.TryGetValue(nameof(NewsUrl).ToLower(), out string newsUrl) 
                    ? newsUrl : DefaultNewsUrl;

                StopParsingOnExisting = !dictionary.TryGetValue(nameof(StopParsingOnExisting).ToLower(), out string stopParsingOnExisting)
                                        || string.Equals(stopParsingOnExisting, "true", StringComparison.InvariantCultureIgnoreCase);

                SaveWithParsing = !dictionary.TryGetValue(nameof(SaveWithParsing).ToLower(), out string saveWithParsing)
                                  || string.Equals(saveWithParsing, "true", StringComparison.InvariantCultureIgnoreCase);

                SkipPages = dictionary.TryGetValue(nameof(SkipPages).ToLower(), out string skipPages) 
                    ? int.Parse(skipPages) : 0;
            }

        }

        public int? PagesToParse { get; set; }

        public int RequestDelayMs { get; set; }

        public string NewsUrl { get; set; }

        public bool StopParsingOnExisting { get; set; }

        public int SkipPages { get; set; }

        public bool SaveWithParsing { get; set; }

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
