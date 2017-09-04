using System.Collections.Generic;
using System.Text;

namespace Onvista.Parser
{
    public class CsvWrapper
    {
        private readonly ICollection<Article> _articles;
        private const string Delimiter = ",";

        public CsvWrapper(ICollection<Article> articles)
        {
            _articles = articles;
        }

        public string GetCsv()
        {
            StringBuilder st = new StringBuilder();
            string headers = string.Join(Delimiter, nameof(Article.Analysis), nameof(Article.Title), nameof(Article.CreatedAt), nameof(Article.Author), nameof(Article.Content));
            st.AppendLine(headers);

            foreach (var article in _articles)
            {
                st.AppendLine(string.Join(Delimiter, article.Analysis, article.Title, $"\"{article.CreatedAt}\"", article.Author, $"\"{article.Content}\""));
            }

            return st.ToString();
        }
    }
}
