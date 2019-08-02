using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawling.Extension;
using WebCrawling.Models;

namespace WebCrawling.Controllers.Document
{
    public class NagarikNewsConfig
    {
        List<EntryModel> dataList = new List<EntryModel>();
        public async Task<List<EntryModel>> GetNagarikNews()
        {
            int i = 1;
            for (; ; )
            {
                string uri = $"https://nagariknews.nagariknetwork.com/category/21?page={i}";
                var htmDocument = await ApiClient.GetAsync(uri);

                var divNodes = htmDocument.DocumentNode.GetHtmlNode("cover-news news-on", "div")
                                .ToList();

                if (divNodes != null)
                {
                    if (await NagarikNewsMainContent(divNodes) != 0)
                        break;
                }

                i++;
            }
            return dataList;
        }
        protected async Task<int> NagarikNewsMainContent(List<HtmlNode> divNodes)
        {
            foreach (var child in divNodes)
            {
                var allowedTags = child.GetHtmlNode("detail-on", "div")
                    .ToList();

                foreach (var tag in allowedTags)
                {
                    var date = tag.SelectNodes(".//p[@class='headline-time']/span").First()
                      .InnerText.ParseNagarikDate();

                    DateTime.TryParse(date, out DateTime publishedDate);

                    if (publishedDate < DateTime.Now.AddDays(-5))
                        return 1;

                    var href = tag.GetAttribute("href");
                    href = $"https://nagariknews.nagariknetwork.com{href}";

                    var doc = await ApiClient.GetAsync(href);
                    var title = tag.Descendants("a").First().InnerText;


                    var contentDiv = doc.DocumentNode.SelectNodes("//div[@id='newsContent']/p");


                    var model = new EntryModel(href, title, date, "Onlinekhabar", "News");
                    dataList.Add(model.PrepareContent(contentDiv));
                }
            }
            return 0;
        }
    }
}
