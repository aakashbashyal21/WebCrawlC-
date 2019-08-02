using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCrawling.Controllers.Document
{
    public class ResponseObj
    {
        public string numPage { get; set; }
        public string newsList { get; set; }
    }

    public class KantipurResponeObj
    {
        public string html { get; set; }
        public string lastDate { get; set; }
        public string requestNewDate { get; set; }
        public string count { get; set; }

    }
}
