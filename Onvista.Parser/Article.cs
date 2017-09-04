﻿using System.Data.Linq.Mapping;

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

        public Article()
        {
        }

        public int Id { get; set; }

        public string Analysis { get; set; }

        public string Title { get; set; }

        [Column(Name = "created_at")]
        public string CreatedAt { get; set; }

        public string Author { get; set; }

        public string Content { get; set; }
    }
}
