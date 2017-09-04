namespace Onvista.Parser
{
    public class Article
    {
        public Article(string analysis, string title, string createdAt, string author, string content)
        {
            Analysis = analysis;
            Title = title;
            CreatedAt = createdAt;
            Author = author;
            Content = content;
        }

        public string Analysis { get; set; }

        public string Title { get; set; }

        public string CreatedAt { get; set; }

        public string Author { get; set; }

        public string Content { get; set; }
    }
}
