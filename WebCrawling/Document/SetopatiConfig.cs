using Hangfire;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebCrawling.Context;
using WebCrawling.Extension;
using WebCrawling.Models;

namespace WebCrawling.Controllers.Document
{
    public interface ISetoPatiConfig
    {
        Task GetSetopati();
    }
    public class SetopatiConfig : ISetoPatiConfig
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<SetopatiConfig> _logger;
        public SetopatiConfig(ILogger<SetopatiConfig> logger,
            AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task GetSetopati()
        {
            int i = 1;
            for (; ; )
            {
                try
                {
                    string uri = $"https://www.setopati.com/samachar/news?page={i}";
                    var htmDocument = await ApiClient.GetAsync(uri);

                    _logger.LogInformation($"Generating data with {uri} Record from setopati");

                    var divNodes = htmDocument.DocumentNode.GetHtmlNode("news-cat-list", "div")
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
                    _logger.LogInformation($"\n ### Running with exception  data with {ex.Message} from setopati \n current url page id {i} ###");
                    Thread.Sleep(500);
                }
            }
        }


        public async Task MainContent(string innerHtml)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(innerHtml);

            var allowedTags = doc.DocumentNode.GetHtmlNode("items", "div")
                   .ToList();

            List<EntryModel> dataList = new List<EntryModel>();
            foreach (var tag in allowedTags)
            {
                var date = tag.SelectNodes(".//div[@class='date-stamp']/span[@class='time-stamp']")?.First()
                  ?.InnerText?.ParseSetoPatiDate();
                
                var href = tag.GetAttribute("href");
                var innerDoc = await ApiClient.GetAsync(href);

                var title = WebUtility.HtmlDecode(tag.GetAttribute("title"));

                var contentDiv = innerDoc.DocumentNode.SelectNodes("//div[@class='editor-box']/p");

                _logger.LogInformation($"\n\n ###### Setopati: Generating {href} with date {date} ######## \n");

                var model = new EntryModel(href, title, date, "Setopati", "News");
                dataList.Add(model.PrepareContent(contentDiv));
            }


            _logger.LogInformation($"\n ### SetoPati: Adding {dataList.Count} items ###");

            await _dbContext.EntryModels.AddRangeAsync(dataList);
            await _dbContext.SaveChangesAsync();
        }


        protected async Task SetopatiMainContent(List<HtmlNode> divNodes)
        {

            List<EntryModel> dataList = new List<EntryModel>();
            foreach (var child in divNodes)
            {


                var allowedTags = child.GetHtmlNode("items", "div")
                    .ToList();

                foreach (var tag in allowedTags)
                {
                    var date = tag.SelectNodes(".//div[@class='date-stamp']/span[@class='time-stamp']")?.First()
                      ?.InnerText?.ParseSetoPatiDate();

                    //DateTime.TryParse(date, out DateTime publishedDate);

                    //if (publishedDate < DateTime.Now.AddMonths(-6))
                    //    return 1;

                    var href = tag.GetAttribute("href");
                    var doc = await ApiClient.GetAsync(href);

                    var title = WebUtility.HtmlDecode(tag.GetAttribute("title"));

                    var contentDiv = doc.DocumentNode.SelectNodes("//div[@class='editor-box']/p");

                    _logger.LogInformation($"\n \n ###### Setopati: Generating {href} with date {date} ######## \n");

                    var model = new EntryModel(href, title, date, "Onlinekhabar", "News");
                    dataList.Add(model.PrepareContent(contentDiv));
                }
            }

            _logger.LogInformation($"SetoPati: Adding {dataList.Count} items");

            await _dbContext.EntryModels.AddRangeAsync(dataList);
            await _dbContext.SaveChangesAsync();

        }

    }
}

