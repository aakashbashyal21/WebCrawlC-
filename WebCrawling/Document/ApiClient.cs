using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebCrawling.Controllers.Document
{
    public class ApiClient
    {
        public static async Task<HtmlDocument> GetAsync(string uri)
        {
            string responseJson = string.Empty;

            var htmlDocument = new HtmlDocument();
            using (var client = new HttpClient())
            {
                var baseUri = uri;
                var html = await client.GetStringAsync(baseUri);

                htmlDocument.LoadHtml(html);
            }
            return htmlDocument;
        }

        public static async Task<HtmlDocument> GetKantipurAsync(string uri)
        {
            string responseJson = string.Empty;

            var htmlDocument = new HtmlDocument();
            using (var client = new HttpClient())
            {
                var baseUri = uri;
                var html = await client.GetStringAsync(baseUri);

                var obj = JsonConvert.DeserializeObject<KantipurResponeObj>(html);

                htmlDocument.LoadHtml(obj.html);

            }
            return htmlDocument;
        }

        public static async Task<HtmlDocument> PostAsync(string uri, int cat, int page)
        {
            string responseJson = string.Empty;
            var htmlDocument = new HtmlDocument();

            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

                var content = new MultipartFormDataContent();

                var values = new[]
                {
                    new KeyValuePair<string, string>("perPage", "20"),
                    new KeyValuePair<string, string>("page", page.ToString()),
                    new KeyValuePair<string, string>("cat", cat.ToString()),
                };

                foreach (var keyValuePair in values)
                {
                    content.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
                }

                var response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {


                    responseJson = await response.Content.ReadAsStringAsync();

                    var obj = JsonConvert.DeserializeObject<ResponseObj>(responseJson);

                    htmlDocument.LoadHtml(obj.newsList);
                }
            }

            return htmlDocument;
        }

        public static async Task<HtmlDocument> PostHamroPatroAsync(string uri, string cat, string token)
        {
            string responseJson = string.Empty;
            var htmlDocument = new HtmlDocument();

            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

                var content = new MultipartFormDataContent();


                var values = new[]
                {
                    new KeyValuePair<string, string>("actionName", "moreComments"),
                    new KeyValuePair<string, string>("nextToken", token),
                    new KeyValuePair<string, string>("category", cat),


                };

                foreach (var keyValuePair in values)
                {
                    content.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
                }

                var response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {


                    responseJson = await response.Content.ReadAsStringAsync();

                    htmlDocument.LoadHtml(responseJson);
                }
            }

            return htmlDocument;
        }
    }
}

//
