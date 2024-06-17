using System;
using System.Diagnostics;
using WebScraping.BigW;
using WebScraping.Bunnings;
using WebScraping.Danmurphy;
using WebScraping.FirstChoiceLiquor;
using WebScraping.PetStock;
using WebScraping.WoolWorths;

class Program
{
    static async Task Main(string[] args)
    {
        
        List<string> urlList = new List<string> { "DanMurphy","WoolWorth","BigW","FirstChoiceLiquor","LiquoreLand","VintageCeller","Bunning","PetStock" };
        for (int i = 0; i < urlList.Count; i++)
        {
            Console.WriteLine($"{i + 1} {urlList[i]}");
        }
        Console.WriteLine("Enter the SN of Ecom Url you want to Crawl.");

        var sn = Console.ReadLine();
        Console.WriteLine("Enter keyword you want to Crawl.");

        var keyword = Console.ReadLine();

        if (sn == "1")
        {
            await CrawlDanMurphyV3.GetDanmurphyData(keyword);
        }
        if (sn == "2")
        {
            await CrawlWoolWorthV2.GetData(keyword);
        }
        if (sn == "3")
        {
            await CrawlBigW.GetBigWData(keyword);
        }
        if (sn == "4")
        {
            await CrawlFirstChoiceLiquor.GetFirstChoiceData(keyword);
        }
        if (sn == "5")
        {
            await CrawlLiquorLand.GetLiquorLandData(keyword);
        }
        if (sn == "6")
        {
            await CrawlVintageCeller.GetVintageCellerData(keyword);
        }
        if (sn == "7")
        {
           // await CrawlBunning.GetBunningData(keyword);
            await CrawlBunning.GetBunningPredefinedRequest(keyword);
        }
        if (sn == "8")
        {
            await CrawlPetStock.GetPetStockData(keyword);
        }

        Console.WriteLine("Do you want to quit? (Y/N)");
        var response = Console.ReadLine();
        if (response?.ToUpper() == "Y")
        {
            return; // Exit the program
        }
        else
        {
            // Restart the app
            await Main(args);
        }

    }
}

