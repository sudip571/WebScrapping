


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PuppeteerSharp;
using WebScraping.Danmurphy.Models;

namespace WebScraping.Danmurphy
{
    public class CrawlDanMurphyV2
    {
        public static async Task<List<TempResultModel>> GetData()
        {
            var responseTasks = new ConcurrentDictionary<string, TaskCompletionSource<string>>();

            var productEndPoint = @"/apis/ui/Search/products";
            var browseEndPoint = @"/apis/ui/Browse";
           List<string> Keywords = new List<string>{ "red wine" }; 
            List<int> PageNumbers = new List<int>{ 1, 2, 3,4 }; 


            // Download the browser if not already present
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            // Launch the browser
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false
            });

            // Open a new page
            var page = await browser.NewPageAsync();

            // Intercept the network request
            await page.SetRequestInterceptionAsync(true);

            page.Request += async (sender, e) =>
            {               
                if ((e.Request.Url.EndsWith(productEndPoint) || e.Request.Url.EndsWith(browseEndPoint)) && e.Request.Method == HttpMethod.Post)
                {
                    // Extract request details
                    var headers = e.Request.Headers;
                    var postData = e.Request.PostData.ToString();

                    // Get cookies
                    var cookies = await page.GetCookiesAsync(e.Request.Url);
                    var cookieHeader = string.Join("; ", cookies.Select(c => $"{c.Name}={c.Value}"));

                    // Store initial request details
                    var initialRequestDetails = new RequestDetails
                    {
                        Url = e.Request.Url,
                        Headers = headers,
                        CookieHeader = cookieHeader,
                        PostData = postData
                    };

                    // Continue the Puppeteer request
                    await e.Request.ContinueAsync();

                    // Run background task to process requests
                    foreach (var keyword in Keywords)
                    {
                        foreach (var pageNumber in PageNumbers)
                        {
                            _ = Task.Run(() => ProcessRequestsInBackground(initialRequestDetails, responseTasks, keyword, pageNumber));

                        }
                    }   

                }
                else
                {
                    // Continue the request without modification
                    await e.Request.ContinueAsync();
                }
            };

            // await page.GoToAsync("https://www.danmurphys.com.au/red-wine/all");
            await page.GoToAsync("https://www.danmurphys.com.au/search?searchTerm=red+wine", timeout: 120000);

          

            // Wait for all background tasks to complete
            await Task.WhenAll(responseTasks.Values.Select(tcs => tcs.Task));

            // Return the collected responses
           
            List<TempResultModel> tempResultModels = new List<TempResultModel>();
            foreach (var task in responseTasks)
            {
                if (task.Value.Task.IsCompletedSuccessfully)
                {
                    var tempKeys = task.Key.Split('-');

                    tempResultModels.Add(new TempResultModel
                    {
                        APIType = tempKeys[2],
                        PageNo = int.Parse(tempKeys[1].Trim()),
                        Data = task.Value.Task.Result,
                        Keyword = tempKeys[0].Trim(),
                    });
                }
            }
            tempResultModels = tempResultModels.OrderBy(x => x.PageNo).ToList();
            int sn = 1;
            foreach (var item in tempResultModels)
            {
                if (item.APIType == "product")
                {
                    try
                    {
                        var productData = JsonSerializer.Deserialize<ProductAPIModel>(item.Data);

                        for (int i = 0; i < productData.Products.Count; i++)
                        {
                            Console.WriteLine($"Keyword: {item.Keyword} Position: {sn}  Title: {productData.Products[i].Name}");
                            sn++;
                        }
                    }
                    catch (Exception)
                    {

                        //
                    }
                    
                }
                else if (item.APIType  == "browse")
                {
                    try
                    {
                        var browseData = JsonSerializer.Deserialize<BrowseAPIModel>(item.Data);

                        for (int i = 0; i < browseData.Bundles.Count; i++)
                        {
                            Console.WriteLine($"Keyword: {item.Keyword} Position: {sn}  Title: {browseData.Bundles[i].Name}");
                            sn++;
                        }
                    }
                    catch (Exception)
                    {

                        //
                    }
                    
                }
            }

            await ClearAllStorageAsync(page);

            await Task.Delay(300);

            await page.CloseAsync();

            await Task.Delay(300);

            // Close the browser
            await browser.CloseAsync();

            return tempResultModels;

        }

        private static async Task ClearAllStorageAsync(IPage page)
        {
            var client = page.Client;

            // Clear cookies
            var cookies = await page.GetCookiesAsync();
            foreach (var cookie in cookies)
            {
                await page.DeleteCookieAsync(cookie);
            }

            // Execute JavaScript to clear local storage and session storage
            await page.EvaluateFunctionOnNewDocumentAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
            //await page.EvaluateFunctionAsync("() => { localStorage.clear(); sessionStorage.clear(); }");

            // Clear cache storage
            await client.SendAsync("Network.clearBrowserCache");

            // Clear indexedDB, webSQL, and cache storage
            await client.SendAsync("Storage.clearDataForOrigin", new
            {
                origin = page.Url,
                storageTypes = "indexeddb,websql,cache_storage"
            });

            // Clear service workers
            //await client.SendAsync("ServiceWorker.unregister", new { scopeURL = page.Url });

            // Clear any other remaining storage
            await client.SendAsync("Storage.clearDataForOrigin", new
            {
                origin = page.Url,
                storageTypes = "all"
            });
        }

        public static  async Task<string> SendRequestAsync(RequestDetails requestDetails, string modifiedPostData)
        {
            var productEndPoint = @"/apis/ui/Search/products";
            var browseEndPoint = @"/apis/ui/Browse";
            using var client = new HttpClient();

            // Set the headers
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestDetails.Url);
            foreach (var header in requestDetails.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Set cookies
            if (!string.IsNullOrEmpty(requestDetails.CookieHeader))
            {
                requestMessage.Headers.Add("Cookie", requestDetails.CookieHeader);
            }

            // Set the payload
            if (!string.IsNullOrEmpty(modifiedPostData))
            {
                requestMessage.Content = new StringContent(modifiedPostData, Encoding.UTF8, "application/json");
            }

            // Send the request and get the response
            var response = await client.SendAsync(requestMessage);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Output the response for debugging purposes
            //Console.WriteLine("Response:");
           // Console.WriteLine(responseContent);

            //if (requestDetails.Url.EndsWith(productEndPoint))
            //{
            //    var productData = JsonSerializer.Deserialize<ProductAPIModel>(responseContent);

            //    for (int i = 0; i < productData.Products.Count; i++)
            //    {
            //        Console.WriteLine($"Position: {i + 1}  Title: {productData.Products[i].Name}");
            //    }
            //}
            //else if (requestDetails.Url.EndsWith(browseEndPoint))
            //{
            //    var browseData = JsonSerializer.Deserialize<BrowseAPIModel>(responseContent);

            //    for (int i = 0; i < browseData.Bundles.Count; i++)
            //    {
            //        Console.WriteLine($"Position: {i + 1}  Title: {browseData.Bundles[i].Name}");
            //    }
            //}

            return responseContent;
        }

        private static string ModifyRequestData(string originalPostData, string keyword, int pageNumber)
        {
            string pattern1 = @"""pageSize"":\d+";
            string pattern2 = @"""PageSize"":\d+";
            string pattern3 = @"""PageNumber"":\d+";
            string pattern4 = @"""pageNumber"":\d+";

            string newPostData = originalPostData;


            //newPostData = Regex.Replace(newPostData, pattern1, "\"pageSize\":20");
            //newPostData = Regex.Replace(newPostData, pattern2, "\"PageSize\":20");

            newPostData = Regex.Replace(newPostData, pattern3, $"\"PageNumber\":{pageNumber}");
            newPostData = Regex.Replace(newPostData, pattern4, $"\"pageNumber\":{pageNumber}");
            newPostData = newPostData.Replace("\"SearchTerm\":\"whiskey\"", $"\"SearchTerm\":{keyword}");
            newPostData = newPostData.Replace("?searchTerm=whiskey", $"?searchTerm={keyword}");
            

            return newPostData;
        }

        private static async Task ProcessRequestsInBackground(RequestDetails initialRequestDetails, ConcurrentDictionary<string, TaskCompletionSource<string>> responseTasks,string keyword, int pageNumber)
        {
            var apiType = initialRequestDetails.Url.EndsWith("/apis/ui/Search/products") ? "product" : "browse";
            var modifiedPostData = ModifyRequestData(initialRequestDetails.PostData, keyword, pageNumber);
            var key = $"{keyword}-{pageNumber}-{apiType}";
            var tcs = new TaskCompletionSource<string>();
            responseTasks[key] = tcs;

            var response = await SendRequestAsync(initialRequestDetails, modifiedPostData);
            tcs.SetResult(response);
        }

    }

    public class TempResultModel
    {
        public int PageNo { get; set; }
        public string Data { get; set; }
        public string APIType { get; set; }
        public string Keyword { get; set; }
    }

}



