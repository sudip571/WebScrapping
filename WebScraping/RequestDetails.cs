using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScraping
{
    public class RequestDetails
    {
        public string Url { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public string CookieHeader { get; set; }
        public string PostData { get; set; }
    }

    public class RetailerSiteDetails
    {
        public List<string> APIEndPoint { get; set; }
        public List<string> Othervalues { get; set; }
        public List<string> Keywords { get; set; }
        public bool IsHeadLess { get; set; } = false;
        public HttpMethod HttpMethod { get; set; } = HttpMethod.Post;
        public string PageUrl { get; set; }
        public string WebSiteName { get; set; }
        public string CheckType { get; set; } = string.Empty;
        public int TimeOut { get; set; } = 1200000;
        public int PageStart { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Dictionary<string, string> ExpectedHeaders { get; set; } = new Dictionary<string, string>();
    }

    public class ResponseDetails
    {
        public string SKUID { get; set; }
        public string Name { get; set; }
    }
}
