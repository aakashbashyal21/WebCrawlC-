using Hangfire;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawling.Context;
using WebCrawling.Controllers.Document;
using WebCrawling.Document;
using WebCrawling.Extension;
using WebCrawling.Models;

namespace WebCrawling.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISetoPatiConfig _setoPatiConfig;
        private readonly INayaPatrikaConfig _nayaPatrikaConfig;
        private readonly IOnlineKhabarConfig _onlineKhabarConfig;
        private readonly INepalAajaConfig _nepalAajaConfig;
        private readonly IKantipurConfig _kantipurConfig;

        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;




        public HomeController(AppDbContext context,
            ILogger<HomeController> logger,
            ISetoPatiConfig setoPatiConfig,
            IOnlineKhabarConfig onlineKhabarConfig,
            INepalAajaConfig nepalAajaConfig,
            IKantipurConfig kantipurConfig,
            INayaPatrikaConfig nayaPatrikaConfig)
        {
            _setoPatiConfig = setoPatiConfig;
            _nayaPatrikaConfig = nayaPatrikaConfig;
            _onlineKhabarConfig = onlineKhabarConfig;
            _nepalAajaConfig = nepalAajaConfig;
            _kantipurConfig = kantipurConfig;

            _context = context;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> List()
        {


            var result = await _context.NewsArticles.ToListAsync();

            var str = new MeasureFrequency().ProcessDocument(result.Select(x => x.Content).ToArray());

            return View(str);
        }
        public async Task<IActionResult> ExtractData()
        {

            _logger.LogInformation(LoggingEvents.ListItems, "Getting Record from Portal");


            //code for extracting from the online khabar, 
            //Document/OnlineKhabarConfig.cs
            await _onlineKhabarConfig.GetOnlineKhabar();

            //code for extracting from the Nayapatrika
            await _nayaPatrikaConfig.GetNayaPatrika();

            //code for extracting from the Setopati
            await _setoPatiConfig.GetSetopati();

            //code for extracting from Kantipur
            await _kantipurConfig.GetKantipur();

            //code for extracting from NepalAaja
            await _nepalAajaConfig.GetNepalAaja();

            return View();
        }


        public async Task<IActionResult> Create()
        {
            var result = await _context.EntryModels.ToListAsync();

            var obj = from r in result
                      select new NewsArticle
                      {
                          CreatedDate = DateTime.Now,
                          Content = r.Content,
                          PublishedDate = Convert.ToDateTime(r.PublishedDate),
                          Href = r.Href,
                          Title = r.Title,
                          Genre = r.Genre,
                          Site = r.Site
                      };

            await _context.NewsArticles.AddRangeAsync(obj);
            await _context.SaveChangesAsync();

            return Redirect("/home");

        }




        public async Task<IActionResult> Crawl()
        {
            return View();
        }




        //source code for crawling the site
        [HttpPost]
        public async Task<IActionResult> DownloadCrawl()
        {
            HtmlWeb hw = new HtmlWeb();


            var hubResult = new List<TermResult>();

            //token array, this array is needed to navigate to filter out the more content from the first page
            string[] tokenArray = { "", "1564555769791_5708098970451968", "1564487902231_5674509943832576", "1564466508320_5707030765109248" };

            for (int i = 0; i < tokenArray.Length; i++)
            {
                List<CrawlFrequency> hrefTags = new List<CrawlFrequency>();

                //call the api client for downloading the link
                HtmlDocument htmlSnippet = await ApiClient.PostHamroPatroAsync("https://www.hamropatro.com/getMethod.php", "ENTERTAINMENT", tokenArray[i]);

                //find  all the link from the h
                var linkedPages = htmlSnippet.DocumentNode.Descendants("a")
                                                  .Select(a => a.GetAttributeValue("href", null))
                                                  .Where(u => !String.IsNullOrEmpty(u));

                foreach (var innerDoc in linkedPages)
                {
                    //visit the detail of the individual link
                    var innerdoc = await ApiClient.GetAsync($"https://www.hamropatro.com{innerDoc}");

                    var div = innerdoc.DocumentNode.SelectSingleNode("//div[@class='read-full']");
                    if (div != null)
                    {
                        //extract the link which redirects to original source of the same news
                        Uri myUri = new Uri(div.Descendants("a")
                                       .Select(a => a.GetAttributeValue("href", null))
                                       .First());
                        string host = myUri.Host;

                        hrefTags.Add(new CrawlFrequency() { Site = host });

                    }

                }

                await _context.CrawlFrequency.AddRangeAsync(hrefTags);
                await _context.SaveChangesAsync();
            }




            return Redirect("/home/listcrawl");
        }

        public async Task<IActionResult> ListCrawl()
        {
            var result = await _context.CrawlFrequency.ToListAsync();

            var query = result.GroupBy(u => u.Site)
                                      .Select(grp => new TermResult
                                      {
                                          Sites = grp.Key.ToString(),
                                          Count = grp.Count()
                                      }).OrderByDescending(o => o.Count).ToList();
            return View(query);
        }

    }
}
