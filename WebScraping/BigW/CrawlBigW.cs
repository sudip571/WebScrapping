using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraping.BigW.Models;

namespace WebScraping.BigW
{
    public class CrawlBigW
    {
        public static async Task GetBigWData(string keyword)
        {
            RetailerSiteDetails retailerSiteDetails = new RetailerSiteDetails()
            {
                APIEndPoint = new List<string> { "/search/v1/search" },
                HttpMethod = HttpMethod.Post,
                IsHeadLess = false,
                Keywords = new List<string> { keyword },
                PageUrl = $@"https://www.bigw.com.au/search?text={keyword}&page=0",
                TimeOut = 120000,
                WebSiteName = "bigw",
                PageStart = 0,
                PageSize = 100
            };
            retailerSiteDetails.ExpectedHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            retailerSiteDetails.ExpectedHeaders.Add("Sec-Fetch-Site", "same-site");
            retailerSiteDetails.ExpectedHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
            var data = await Crawler.GetCrawledData<BigWResponse>(retailerSiteDetails);
            int count = 1;
            foreach (var item in data.Take(100))
            {
                Console.WriteLine($"SN: {count} SKUID: {item.SKUID}  Title: {item.Name}");
                count++;
            }
        }
    }
}
