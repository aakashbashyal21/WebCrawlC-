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
    public interface INayaPatrikaConfig
    {
        Task GetNayaPatrika();
    }
    public class NayaPatrikaConfig : INayaPatrikaConfig
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<NayaPatrikaConfig> _logger;
        public NayaPatrikaConfig(ILogger<NayaPatrikaConfig> logger,
            AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
        }


        public async Task GetNayaPatrika()
        {
            int i = 1;
            for (; ; )
            {
                try
                {
                    string uri = $"https://nayapatrikadaily.com/ajax/pagination.php";

                    var htmDocument = await ApiClient.PostAsync(uri, 2, i);


                    var divNodes = htmDocument.DocumentNode.GetHtmlNode("md-newsbox3", "div")
                                    .ToList();


                    _logger.LogInformation($"\n \n ######## Generating data with {uri} Record from Nayapatrika with cat {2}, page {i} ######## \n \n");

                    if (divNodes != null)
                    {
                        foreach (var child in divNodes)
                        {
                      
                            BackgroundJob.Enqueue(() => MainContent(child.InnerHtml));
                        }
                    }

                    i++;
                }
                catch (Exception ex)
                {

                    _logger.LogInformation($"Running with exception  data with {ex.Message} from Nayapatrika");
                }
            }
        }
        public async Task MainContent(string innerHtml)
        {
            List<EntryModel> dataList = new List<EntryModel>();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(innerHtml);

            var nodeList = doc.DocumentNode;

            var href = nodeList.GetAttribute("href");
            var innerDoc = await ApiClient.GetAsync(href);

            var node = innerDoc.DocumentNode;

            var date = node.SelectNodes(".//div[@class='date']/p")
               ?.First()
               ?.InnerText.ParseOnlineKhabarDate();


            var nodes = node.Descendants("section")?
                .First()?.InnerText?.Trim().Replace("\r\n", string.Empty);

            var title = node.SelectNodes("//article/header/h2")?.First()
                ?.InnerText;

            _logger.LogInformation($"Nayapatrika: Generating {href} \n with date {date}");

            var model = new EntryModel(href, title, date, "Nayapatrika", "News");
            dataList.Add(model.PrepareContent(null, nodes));

            _logger.LogInformation($"Nayapatrika: Adding {dataList.Count} items");

            await _dbContext.EntryModels.AddRangeAsync(dataList);
            await _dbContext.SaveChangesAsync();
        }
    }
}

