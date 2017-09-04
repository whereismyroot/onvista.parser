using System;
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
                              $"Already exists: {newsArticles.Count(x => x.ResultType == ParsingResultType.AlreadyExists)} ");

            if (pendingForSave.Count > 0)
            {
                Console.WriteLine("Saving parser results...");
                parser.SaveParsingResults(pendingForSave);

                Console.WriteLine("Done");
            }

            //$"Error: {newsArticles.Count(x => x.ResultType == ParsingResultType.Error)} "

            //string csv = new CsvWrapper(newsArticles).GetCsv();
            //string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"articles-{DateTime.Now:dd.MM.yy HH.mm}.csv");

            //File.WriteAllText(filePath, csv);

            //Console.WriteLine($"Results were written to {filePath}");


        }
    }
}
