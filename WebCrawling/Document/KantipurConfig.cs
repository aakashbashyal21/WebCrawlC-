using Hangfire;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebCrawling.Context;
using WebCrawling.Extension;
using WebCrawling.Models;

namespace WebCrawling.Controllers.Document
{
    public interface IKantipurConfig
    {
        Task GetKantipur();
    }
    public class KantipurConfig : IKantipurConfig
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<IKantipurConfig> _logger;
        public KantipurConfig(ILogger<IKantipurConfig> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        List<EntryModel> dataList = new List<EntryModel>();
        public async Task GetKantipur()
        {
            int i = 0;
            for (; ; )
            {
                var date = DateTime.Now.AddDays(-i).ToString("yyyy/MM/dd");
                try
                {
                    string uri = $"https://www.kantipurdaily.com/news/{date}?json=true";

                    var htmDocument = await ApiClient.GetKantipurAsync(uri);

                    _logger.LogInformation($"\n \n ### Generating data with {uri} Record from Kantipur with date {date} ###");

                    var divNodes = htmDocument.DocumentNode
                      .Descendants("article").ToList();

                    if (divNodes != null)
                    {
                        foreach (var child in divNodes)
                            BackgroundJob.Enqueue(() => MainContent(child.InnerHtml));
                    }

                    i++;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Running with exception  data with {ex.Message} from Kantipur: page parameter value {date}");
                }
            }
        }



        public async Task MainContent(string innerHtml)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(innerHtml);

            var nodeList = doc.DocumentNode;

            var href = nodeList.GetAttribute("href");

            var innerDocument = await ApiClient.GetAsync(href);

            var node = innerDocument.DocumentNode;

            var date = node.SelectNodes("//article/div/div/time")
               ?.First()
               ?.InnerText;

            try
            {

                date = System.Net.WebUtility.HtmlDecode(date).ParseKantipurDate();
                DateTime.TryParse(date, out DateTime publishedDate);

                if (publishedDate < DateTime.Now.AddMonths(-7))
                    return;
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"\n \n \n ### Kantipur: error parse format {href} with date {System.Net.WebUtility.HtmlDecode(date)} ###\n\n");

                Thread.Sleep(500);

                throw;
            }




            var paragraphs = node.SelectNodes("//div[contains(@class, 'description')]")
                ?.First()?.InnerText;

            var title = node.SelectNodes("//div[contains(@class, 'article-header')]/h1")
                ?.First()
                ?.InnerText;

            _logger.LogInformation($"\n \n ### Kantipur: Generating {href} with date {date} ###\n\n");

            var model = new EntryModel(href, title, date, "Kantipur", "News");
            dataList.Add(model.PrepareContent(null, paragraphs));

            _logger.LogInformation($"Kantipur: Adding {dataList.Count} items");

            await _dbContext.EntryModels.AddRangeAsync(dataList);
            await _dbContext.SaveChangesAsync();

        }



        public async Task<int> KantipurMainContent(List<HtmlNode> divNodes)
        {
            List<EntryModel> dataList = new List<EntryModel>();
            foreach (var tag in divNodes)
            {

                var href = tag.GetAttribute("href");

                var doc = await ApiClient.GetAsync(href);

                var node = doc.DocumentNode;

                var date = node.SelectNodes("//article[contains(@class, 'normal')]/div/div/time")
                   ?.First()
                   ?.InnerText;

                date = System.Net.WebUtility.HtmlDecode(date).ParseKantipurDate();

                //DateTime.TryParse(date, out DateTime publishedDate);

                //if (publishedDate < DateTime.Now.AddMonths(-6))
                //    return 1;


                var paragraphs = node.SelectNodes("//div[contains(@class, 'description')]")
                    ?.First()?.InnerText;

                var title = node.SelectNodes("//div[contains(@class, 'article-header')]/h1")
                    ?.First()
                    ?.InnerText;

                _logger.LogInformation($"Kantipur: Generating {href} \n with {title}: \n date {date}");

                var model = new EntryModel(href, title, date, "Onlinekhabar", "News");
                dataList.Add(model.PrepareContent(null, paragraphs));
            }
            _logger.LogInformation($"Kantipur: Adding {dataList.Count} items");

            await _dbContext.EntryModels.AddRangeAsync(dataList);
            await _dbContext.SaveChangesAsync();
            return 0;
        }
    }

}
