using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper.FluentMap;
using Onvista.Parser.Data;

namespace Onvista.Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            FluentMapper.Initialize(config =>
            {
                config.AddMap(new ArticleMap());
            });

            OnvistaArticlesParsingParameters parserParameters = new OnvistaArticlesParsingParameters(args);

            ArticlesRepository repository = new ArticlesRepository();
            
            ArticlesParser parser = new ArticlesParser(repository);
            var newsArticles = parser.ParseArticles(parserParameters);

            var pendingForSave = newsArticles.Where(x => x.ResultType == ParsingResultType.PendingForSave).ToList();

            Console.WriteLine($"{newsArticles.Count} Articles were parsed.{Environment.NewLine}" +
                              $"PendingForSave: {pendingForSave.Count} | " +
                              $"Saved: {newsArticles.Count(x => x.ResultType == ParsingResultType.Saved)} | " +
                              $"Already exists: {newsArticles.Count(x => x.ResultType == ParsingResultType.AlreadyExists)} ");

            if (pendingForSave.Count > 0)
            {
                Console.WriteLine("Saving parser results...");
                parser.SaveParsingResults(pendingForSave);
                
                Console.WriteLine("Done");
            }

            SaveParsedArticlesToCsv(newsArticles);
        }

        private static void SaveParsedArticlesToCsv(ICollection<ParsingResult<Article>> articles)
        {
            string csv = new CsvWrapper(articles).GetCsv();
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"articles-{DateTime.Now:dd.MM.yy HH.mm}.csv");

            File.WriteAllText(filePath, csv, System.Text.Encoding.UTF8);

            Console.WriteLine($"Results were written to {filePath}");
        }
    }
}
