using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraping.FirstChoiceLiquor.Models;

namespace WebScraping.FirstChoiceLiquor
{
  
    public class CrawlVintageCeller
    {
        public static async Task GetVintageCellerData(string keyword)
        {
            RetailerSiteDetails retailerSiteDetails = new RetailerSiteDetails()
            {
                APIEndPoint = new List<string> { "/api/search/vc/nsw", "/api/products/vc/nsw" },
                Othervalues = ["facets="],
                HttpMethod = HttpMethod.Get,
                IsHeadLess = false,
                Keywords = new List<string> { keyword },
                PageUrl = $@"https://www.vintagecellars.com.au/search?q={keyword}",
                TimeOut = 120000,
                WebSiteName = "vintage",
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
