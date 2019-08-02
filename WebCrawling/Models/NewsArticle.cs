using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCrawling.Models
{
    public class NewsArticle
    {
        public int Id { get; set; }
        public string Href { get; set; }
        public string Title { get; set; }
        public DateTime? PublishedDate { get; set; }

        public string Content { get; set; }
        public string Site { get; set; }
        public string Genre { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
