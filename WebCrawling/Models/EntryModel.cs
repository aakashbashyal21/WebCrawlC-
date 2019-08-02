using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawling.Models
{
    public class EntryModel
    {
        public EntryModel()
        {

        }

        public EntryModel(string href, string title, string date, string Site, string Genre)
        {
            this.Href = href;
            this.Title = title;
            this.PublishedDate = date;
            this.Site = Site;
            this.Genre = Genre;
        }

        public int Id { get; set; }
        public string Href { get; set; }
        public string Title { get; set; }
        public string PublishedDate { get; set; }

        public string Content { get; set; }
        public string Site { get; set; }
        public string Genre { get; set; }


        public static readonly List<string> StopWords = System.IO.File.ReadAllLines("c:\\stopwords01.txt").ToList();

        public EntryModel PrepareContent(HtmlNodeCollection contentDiv = null, string content = null)
        {


            StringBuilder fContent = new StringBuilder();

            if (string.IsNullOrEmpty(content))
            {
                foreach (HtmlNode node in contentDiv)
                {

                    fContent.Append(node.InnerText);
                }
            }
            else
            {
                fContent.Append(content);
            }


            var model = new EntryModel()
            {
                Href = this.Href,
                Title = this.Title,
                Content = WebUtility.HtmlDecode(fContent.ToString()),
                PublishedDate = this.PublishedDate,
                Genre = this.Genre,
                Site = this.Site
            };

            return model;
        }
    }
}
