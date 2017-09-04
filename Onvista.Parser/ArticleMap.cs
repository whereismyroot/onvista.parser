using Dapper.FluentMap.Mapping;

namespace Onvista.Parser
{
    public class ArticleMap : EntityMap<Article>
    {
        public ArticleMap()
        {
            Map(p => p.CreatedAt)
                .ToColumn("created_at");
        }
    }
}
