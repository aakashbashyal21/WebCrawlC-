using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCrawling.Extension
{
    public static class ParseExtensions
    {
        public static IEnumerable<HtmlNode> GetHtmlNode(this HtmlNode htmlNode, string className, string selector)
        {
            var divNode = htmlNode.Descendants(selector)
                     .Where(node => node.GetAttributeValue("class", "")
                     .Contains(className));

            return divNode;
        }

        public static string GetAttribute(this HtmlNode htmlNode, string attrib)
        {
            var href = htmlNode.Descendants("a")
                          .Select(node => node.GetAttributeValue(attrib, "")).FirstOrDefault();

            return href;
        }
    }
}
