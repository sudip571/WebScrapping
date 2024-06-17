using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraping.Danmurphy.Models;
using WebScraping.FirstChoiceLiquor.Models;

namespace WebScraping.Danmurphy
{
   
    public class CrawlDanMurphyV3
    {
        public static async Task GetDanmurphyData(string keyword)
        {
            RetailerSiteDetails retailerSiteDetails = new RetailerSiteDetails()
            {
                APIEndPoint = new List<string> { "/apis/ui/ProductGroup/Products", "/apis/ui/Browse", "/apis/ui/Search/products" },
                HttpMethod = HttpMethod.Post,
                IsHeadLess = false,
                Keywords = new List<string> { keyword },
                PageUrl = $@"https://www.danmurphys.com.au/search?searchTerm={keyword}",
                TimeOut = 120000,
                WebSiteName = "danmurphy",
                PageStart = 1,
                PageSize = 30,
                CheckType = "contains"
            };
            //retailerSiteDetails.ExpectedHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            //retailerSiteDetails.ExpectedHeaders.Add("Sec-Fetch-Site", "same-origin");
            //retailerSiteDetails.ExpectedHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");

            var data = await Crawler.GetCrawledData<DanMurphyResponse>(retailerSiteDetails);
            int count = 1;
            foreach (var item in data.Take(100))
            {
                Console.WriteLine($"SN: {count} SKUID: {item.SKUID}  Title: {item.Name}");
                count++;
            }
        }
    }
}
