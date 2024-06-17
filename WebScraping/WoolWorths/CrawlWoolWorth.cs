using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerExtraSharp;
using PuppeteerSharp;
using WebScraping.Danmurphy;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerExtraSharp.Plugins.AnonymizeUa;
using WebScraping.WoolWorths.Models;

namespace WebScraping.WoolWorths
{
    public class CrawlWoolWorth
    {
        public static async Task GetData()
        {
            var productEndPoint = @"/apis/ui/Search/products";
            var pageUrl = @"https://www.woolworths.com.au/shop/search/products?searchTerm=noodles";

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false
            });
            
            var page = await browser.NewPageAsync();

            var responseTasks = new ConcurrentDictionary<string, TaskCompletionSource<string>>();


            await page.SetRequestInterceptionAsync(true);

            page.Request += async (sender, e) =>
            {
                if (e.Request.Url.EndsWith(productEndPoint) && e.Request.Method == HttpMethod.Post)
                {

                    var postData = e.Request.PostData.ToString();
                    //var newPostData = postData.ToString().Replace("\"pageSize\":36", "\"pageSize\":100").Replace("\"PageSize\":36", "\"PageSize\":100");
                    int pageNo = 1;

                    string pattern1 = @"""pageSize"":\d+";
                    string pattern2 = @"""PageSize"":\d+";
                    string pattern3 = @"""PageNumber"":\d+";
                    string pattern4 = @"""pageNumber"":\d+";

                    string newPostData = "";


                    //newPostData = Regex.Replace(postData, pattern1, "\"pageSize\":20");
                    //newPostData = Regex.Replace(newPostData, pattern2, "\"PageSize\":20");

                    newPostData = Regex.Replace(newPostData, pattern3, $"\"PageNumber\":{pageNo}");
                    newPostData = Regex.Replace(newPostData, pattern4, $"\"pageNumber\":{pageNo}");

                    // Create a TaskCompletionSource for this request
                    var tcs = new TaskCompletionSource<string>();
                    responseTasks.TryAdd(e.Request.Url, tcs);

                    // Continue the request with modified post data
                    await e.Request.ContinueAsync(new Payload
                    {
                        Method = e.Request.Method,
                        Headers = e.Request.Headers,
                        PostData = newPostData
                    });
                }
                else
                {
                    // Continue the request without modification
                    await e.Request.ContinueAsync();
                }
            };

            await page.GoToAsync(pageUrl, timeout: 120000,new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle2 });

            // Capture the responses
            page.Response += async (sender, e) =>
            {
                if (e.Response.Url.EndsWith(productEndPoint) && e.Response.Request.Method == HttpMethod.Post)
                {
                    try
                    {
                        var responseData = await e.Response.TextAsync();
                        if (responseTasks.TryRemove(e.Response.Request.Url, out var tcs))
                        {
                            tcs.SetResult(responseData);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (responseTasks.TryRemove(e.Response.Request.Url, out var tcs))
                        {
                            tcs.SetException(ex);
                        }
                    }
                }
            };

            // Wait for the API responses
            var allResponses = await WaitForAllResponsesAsync(responseTasks, TimeSpan.FromMinutes(2));

            
            foreach (var response in allResponses)
            {
                if (response.Key.EndsWith(productEndPoint))
                {
                    var productData = JsonSerializer.Deserialize<ProductAPIModel>(response.Value);

                    if (productData is not null)
                    {
                        var tempData = productData.Products.SelectMany(p => p.ProductDetail)
                                        .Select(p => new
                                        {
                                            Name = p.DisplayName,
                                            SKUID = p.SKUID
                                        })
                                        .ToList();


                        Console.WriteLine($"Total data: {productData.SearchResultsCount}");
                        for (int i = 0; i < tempData.Count; i++)
                        {
                            Console.WriteLine($"Position: {i + 1} SKUID: {tempData[i].SKUID}  Title: {tempData[i].Name}");
                        }

                        //to display first ProductDetail from ProductsModel
                        //var tempData = productData.Products.Select(p => p.ProductDetail.FirstOrDefault())
                        //    .Where(p => p != null)
                        //    .Select(p => new
                        //    {
                        //        Name = p.DisplayName,
                        //        SKUID = p.SKUID
                        //    })
                        //    .ToList();
                    }
                }

                else
                {
                    Console.WriteLine($"URL: {response.Key}");
                    Console.WriteLine("API Response:");
                    Console.WriteLine(response.Value);
                }

            }

            await ClearAllStorageAsync(page);

            await Task.Delay(300);

            await page.CloseAsync();

            await Task.Delay(300);
          
            await browser.CloseAsync();
        }


        private static async Task ClearAllStorageAsync(IPage page)
        {
            var client = page.Client;
           
            var cookies = await page.GetCookiesAsync();
            foreach (var cookie in cookies)
            {
                await page.DeleteCookieAsync(cookie);
            }

           
            await page.EvaluateFunctionOnNewDocumentAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
            //await page.EvaluateFunctionAsync("() => { localStorage.clear(); sessionStorage.clear(); }");

           
            await client.SendAsync("Network.clearBrowserCache");

          
            await client.SendAsync("Storage.clearDataForOrigin", new
            {
                origin = page.Url,
                storageTypes = "indexeddb,websql,cache_storage"
            });

            await client.SendAsync("Storage.clearDataForOrigin", new
            {
                origin = page.Url,
                storageTypes = "all"
            });
        }

        //private static async Task<List<string>> WaitForAllResponsesAsync(ConcurrentDictionary<string, TaskCompletionSource<string>> responseTasks, TimeSpan timeout)
        //{
        //    var responses = new List<string>();

        //    foreach (var task in responseTasks.Values)
        //    {
        //        try
        //        {
        //            using (var cts = new CancellationTokenSource(timeout))
        //            {
        //                cts.Token.Register(() => task.TrySetCanceled(), useSynchronizationContext: false);
        //                responses.Add(await task.Task);
        //            }
        //        }
        //        catch (OperationCanceledException)
        //        {
        //            Console.WriteLine("Timed out waiting for response.");
        //        }
        //    }

        //    return responses;
        //}
        private static async Task<Dictionary<string, string>> WaitForAllResponsesAsync(ConcurrentDictionary<string, TaskCompletionSource<string>> responseTasks, TimeSpan timeout)
        {
            var responses = new Dictionary<string, string>();

            foreach (var kvp in responseTasks)
            {
                try
                {
                    using (var cts = new CancellationTokenSource(timeout))
                    {
                        cts.Token.Register(() => kvp.Value.TrySetCanceled(), useSynchronizationContext: false);
                        var responseData = await kvp.Value.Task;
                        responses.Add(kvp.Key, responseData);
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"Timed out waiting for response from {kvp.Key}");
                }
            }

            return responses;
        }
    }
}


