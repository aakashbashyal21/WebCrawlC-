using Hangfire;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebCrawling.Context;
using WebCrawling.Extension;
using WebCrawling.Models;

namespace WebCrawling.Controllers.Document
{
    public interface INepalAajaConfig
    {
        Task GetNepalAaja();
    }
    public class NepalAajaConfig : INepalAajaConfig
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<NepalAajaConfig> _logger;
        public NepalAajaConfig(ILogger<NepalAajaConfig> logger, AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task GetNepalAaja()
        {
            int i = 1;
            for (; ; )
            {

                string uri = $"https://www.nepalaaja.com/categories/news?page={i}";

                try
                {
                    var htmDocument = await ApiClient.GetAsync(uri);

                    _logger.LogInformation($"Generating data with {uri} Record from nepalaaja");

                    var divNodes = htmDocument.DocumentNode
                                    .Descendants("article").ToList();


                    if (divNodes != null)
                    {
                        //foreach (var child in divNodes)
                        //    await MainContent(child.InnerHtml);
                        foreach (var child in divNodes)
                            BackgroundJob.Enqueue(() => MainContent(child.InnerHtml));

                    }

                    i++;

                }

                catch (Exception ex)
                {
                    _logger.LogInformation($"Running with exception  data with {ex.Message} from Kantipur: page url {uri}");
                }
            }
        }

        public async Task MainContent(string innerHtml)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(innerHtml);

            List<EntryModel> dataList = new List<EntryModel>();


            var outernode = doc.DocumentNode;

            var href = outernode.GetAttribute("href");

            if (href.Contains("sponsor"))
                return;

            var innerContent = await ApiClient.GetAsync(href);

            var innernode = innerContent.DocumentNode;

            var date = string.Empty;
            try
            {
                date = innernode.SelectNodes("//article[contains(@class, 'post')]/div[contains(@class, 'entry-meta')]/span").First()?.InnerText.ParseNagarikDate();

                DateTime.TryParse(date, out DateTime publishedDate);

                if (publishedDate < DateTime.Now.AddMonths(-6))
                    return;

            }

            catch (Exception ex)
            {
                _logger.LogInformation($"\n \n \n ### Nepal aaja: error parse format {href} with date {date} ###\n\n");

                Thread.Sleep(500);

                throw;

            }


            var nodes = innernode.SelectNodes("//div[contains(@class, 'entry-content')]/p")?.First()?.InnerText?.Trim();

            var title = innernode.SelectNodes("//article/div/h2")?.First()
                ?.InnerText?.Trim();

            _logger.LogInformation($"NepalAaja: Generating {href} with  date {date}");

            var model = new EntryModel(href, title, date, "Nepalaaja", "News");
            dataList.Add(model.PrepareContent(null, nodes));

            await _dbContext.EntryModels.AddRangeAsync(dataList);
            await _dbContext.SaveChangesAsync();

        }


        protected async Task<int> NepalAajaMainContent(List<HtmlNode> divNodes)
        {
            try
            {
                List<EntryModel> dataList = new List<EntryModel>();
                foreach (var tag in divNodes)
                {
                    var href = tag.GetAttribute("href");
                    var doc = await ApiClient.GetAsync(href);

                    var node = doc.DocumentNode;


                    var date = node.SelectNodes("//article/div/span")
                       ?.First()
                       ?.InnerText.ParseNagarikDate();

                    DateTime.TryParse(date, out DateTime publishedDate);

                    if (publishedDate < DateTime.Now.AddMonths(-6))
                        return 1;



                    var nodes = node.SelectNodes("//article[contains(@class, 'post')]/div/div/div")?.First()?.InnerText?.Trim();

                    var title = node.SelectNodes("//article/div/h2")?.First()
                        ?.InnerText?.Trim();

                    _logger.LogInformation($"NepalAaja: Generating {href} \n with {title}: \n date {date}");

                    var model = new EntryModel(href, title, date, "Onlinekhabar", "News");
                    dataList.Add(model.PrepareContent(null, nodes));
                }

                _logger.LogInformation($"NepalAaja: Adding {dataList.Count} items");

                _dbContext.EntryModels.AddRange(dataList);
                _dbContext.SaveChanges();

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"NepalAaja: Exception {ex.Message}");
                throw;
            }
        }

    }
}

