using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MySql.Data.MySqlClient;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Onvista.Parser.Data
{
    public class ArticlesRepository : IArticlesRepository
    {
        private const string TableName = "articles";
        private readonly MySqlConnection _connection;

        public ArticlesRepository()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["Onvista.Parser"].ConnectionString;
            _connection = new MySqlConnection(connectionString);
        }

        public Article GetEntity(int id)
        {
            return _connection.Get<Article>(id);
        }

        public ICollection<Article> GetEntities(string where)
        {
            string whereClause = string.IsNullOrEmpty(where) ? string.Empty : $"where {where}";
            string sql = $"select id, title, analysis, author, content, created_at as CreatedAt from articles {whereClause}";
            var result = _connection.Query<Article>(sql) ?? new List<Article>();

            return result.ToList();
        }

        public void Insert(Article entity)
        {
            string sql = "insert into articles (title, analysis, author, content, created_at)" +
                         "values (@title, @analysis, @author, @content, @created_at)";

            _connection.Execute(sql,
                new
                {
                    title = entity.Title,
                    analysis = entity.Analysis,
                    author = entity.Author,
                    content = entity.Content,
                    created_at = entity.CreatedAt
                });
        }
    }
}
