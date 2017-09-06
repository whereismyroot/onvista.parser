using System.Collections.Generic;
using System.Text;

namespace Onvista.Parser
{
    public class CsvWrapper
    {
        private readonly ICollection<ParsingResult<Article>> _articles;
        private const string Delimiter = ",";

        public CsvWrapper(ICollection<ParsingResult<Article>> articles)
        {
            _articles = articles;
        }

        public string GetCsv()
        {
            StringBuilder st = new StringBuilder();
            string headers = string.Join(Delimiter, nameof(Article.Analysis), nameof(Article.Title), nameof(Article.RelativeUrl), nameof(Article.CreatedAt), nameof(Article.Author), nameof(ParsingResult<Article>.ResultType), nameof(Article.Content));
            st.AppendLine(headers);

            foreach (var article in _articles)
            {
                st.AppendLine(string.Join(Delimiter, article.Entity.Analysis, $"\"{article.Entity.Title}\"", article.Entity.RelativeUrl, $"\"{article.Entity.CreatedAt}\"", $"\"{article.Entity.Author}\"", article.ResultType, $"\"{article.Entity.Content.Replace("\"", "\"\"")}\""));
            }

            return st.ToString();
        }
    }
}
