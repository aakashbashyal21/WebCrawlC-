using Hangfire;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawling.Context;
using WebCrawling.Extension;
using WebCrawling.Models;

namespace WebCrawling.Controllers.Document
{
    public interface IOnlineKhabarConfig
    {
        Task GetOnlineKhabar();
    }
    public class OnlineKhabarConfig : IOnlineKhabarConfig
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<OnlineKhabarConfig> _logger;
        public OnlineKhabarConfig(ILogger<OnlineKhabarConfig> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task GetOnlineKhabar()
        {
            int i = 1;
            for (; ; )
            {
                try
                {
                    string uri = $"https://www.onlinekhabar.com/content/news/page/{i}";
                    var htmDocument = await ApiClient.GetAsync(uri);

                    _logger.LogInformation($"Generating data with {uri} Record from Onlinekhabar");

                    var divNodes = htmDocument.DocumentNode.GetHtmlNode("list_child_wrp", "div")
                                    .ToList();

                    if (divNodes != null)
                    {
                        foreach (var child in divNodes)
                            BackgroundJob.Enqueue(() => MainContent(child.InnerHtml));
                    }

                    i++;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Running with exception  data with {ex.Message} from Onlinekhabar");
                }
            }
        }

        public async Task MainContent(string innerHtml)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(innerHtml);

            var allowedTags = doc.DocumentNode.GetHtmlNode("item__wrap", "div")
                   .ToList();

            List<EntryModel> dataList = new List<EntryModel>();

            foreach (var tag in allowedTags)
            {

                var href = tag.GetAttribute("href");
                var maindoc = await ApiClient.GetAsync(href);

                var node = maindoc.DocumentNode;

                var date = node.SelectNodes(".//div[@class='post__time']/span")?.First()
                    ?.InnerText?.ParseOnlineKhabarDate();

                //DateTime.TryParse(date, out DateTime publishedDate);

                //if (publishedDate < DateTime.Now.AddMonths(-6))
                //    return 1;

                var contentDiv = maindoc.DocumentNode.SelectNodes("//div[@class='ok18-single-post-content-wrap']/p");


                var nodes = maindoc.DocumentNode.Descendants(0)
                                                 .Where(n => n.HasClass("ok18-single-post-content-wrap"))?.First()?.InnerText;

                var title = node.SelectNodes("//div[@class='nws__title--card']/h2")?.First()
                    ?.InnerText;

                _logger.LogInformation($"Onlinekhabar: Generating {href} \n with date {date}");

                var model = new EntryModel(href, title, date, "Onlinekhabar", "News");
                dataList.Add(model.PrepareContent(null, nodes));


                _logger.LogInformation($"Added {dataList.Count} to list items");


            }
            _logger.LogInformation($"Onlinekhabar: Adding {dataList.Count} items");

            await _dbContext.EntryModels.AddRangeAsync(dataList);
            await _dbContext.SaveChangesAsync();
        }
    }
}

