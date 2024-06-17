using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraping.Bunnings.Models;

namespace WebScraping.PetStock
{
  
    public class CrawlPetStock
    {
        public static async Task GetPetStockData(string keyword)
        {
            RetailerSiteDetails retailerSiteDetails = new RetailerSiteDetails()
            {
                APIEndPoint = new List<string> { "/1/indexes/*/queries" },
                HttpMethod = HttpMethod.Post,
                IsHeadLess = false,
                Keywords = new List<string> { keyword },
                PageUrl = $@"https://www.petstock.com.au/search?query={keyword}",
                TimeOut = 120000,
                WebSiteName = "petstock",
                PageStart = 0,
                PageSize = 60,
                CheckType = "contains"
            };

            var data = await Crawler.GetCrawledData<ResponseDetails>(retailerSiteDetails);
            int count = 1;
            foreach (var item in data.Take(100))
            {
                Console.WriteLine($"SN: {count} SKUID: {item.SKUID}  Title: {item.Name}");
                count++;
            }
        }
    }
}
