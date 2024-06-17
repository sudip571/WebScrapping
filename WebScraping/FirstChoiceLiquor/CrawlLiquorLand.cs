using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraping.FirstChoiceLiquor.Models;

namespace WebScraping.FirstChoiceLiquor
{
    
    public class CrawlLiquorLand
    {
        public static async Task GetLiquorLandData(string keyword)
        {
            RetailerSiteDetails retailerSiteDetails = new RetailerSiteDetails()
            {
                APIEndPoint = new List<string> { "/api/search/ll/nsw" },
                Othervalues = ["facets="],
                HttpMethod = HttpMethod.Get,
                IsHeadLess = false,
                Keywords = new List<string> { keyword },
                PageUrl = $@"https://www.liquorland.com.au/search?q={keyword}",
                TimeOut = 120000,
                WebSiteName = "liquoreland",
                PageStart = 1,
                PageSize = 60,
                CheckType = "contains"
            };
            retailerSiteDetails.ExpectedHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            retailerSiteDetails.ExpectedHeaders.Add("Sec-Fetch-Site", "same-origin");
            retailerSiteDetails.ExpectedHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");

            var data = await Crawler.GetCrawledData<FirstChoiceLiquorResponse>(retailerSiteDetails);
            int count = 1;
            foreach (var item in data.Take(100))
            {
                Console.WriteLine($"SN: {count} SKUID: {item.SKUID}  Title: {item.Name}");
                count++;
            }
        }
    }
}
