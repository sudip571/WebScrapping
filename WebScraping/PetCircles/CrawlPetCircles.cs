using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraping.Bunnings.Models;

namespace WebScraping.PetCircles
{
    
    public class CrawlPetCircles
    {
        public static async Task GetBunningData(string keyword)
        {
            RetailerSiteDetails retailerSiteDetails = new RetailerSiteDetails()
            {
                APIEndPoint = new List<string> { "/v1/coveo/search" },
                HttpMethod = HttpMethod.Post,
                IsHeadLess = false,
                Keywords = new List<string> { keyword },
                PageUrl = $@"https://www.bunnings.com.au/search/products?q={keyword}",
                TimeOut = 120000,
                WebSiteName = "bunnings",
                PageStart = 1,
                PageSize = 60,
                //CheckType = "contains"
            };

            var data = await Crawler.GetCrawledData<BunningResponse>(retailerSiteDetails);
            int count = 1;
            foreach (var item in data)
            {
                Console.WriteLine($"SN: {count} SKUID: {item.SKUID}  Title: {item.Name}");
                count++;
            }
        }
    }
}
