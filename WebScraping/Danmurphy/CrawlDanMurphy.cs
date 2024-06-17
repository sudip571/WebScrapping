
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PuppeteerSharp;
using WebScraping.Danmurphy.Models;

namespace WebScraping.Danmurphy
{
    public class CrawlDanMurphy
    {
        public static async Task GetData()
        {
            var productEndPoint = @"/apis/ui/Search/products";
            var browseEndPoint = @"/apis/ui/Browse";

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
            
            // A dictionary to store responses
            var responses = new Dictionary<string, TaskCompletionSource<string>>();

            // Intercept the network request
            await page.SetRequestInterceptionAsync(true);

            page.Request += async (sender, e) =>
            {
                if ((e.Request.Url.EndsWith(productEndPoint) || e.Request.Url.EndsWith(browseEndPoint)) && e.Request.Method == HttpMethod.Post)
                {
                    var postData = e.Request.PostData;
                    var newPostData = postData.ToString().Replace("\"pageSize\":24", "\"pageSize\":100").Replace("\"PageSize\":24", "\"PageSize\":100");

                    // Add a task completion source for this request
                    var tcs = new TaskCompletionSource<string>();
                    responses[e.Request.Url] = tcs;

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


            // Go to the page that makes the API call
            // await page.GoToAsync("https://www.danmurphys.com.au/red-wine/all");
            await page.GoToAsync("https://www.danmurphys.com.au/search?searchTerm=whiskey", timeout: 60000);

            // Wait for network requests to complete           
            //await Task.Delay(3000);

            // Capture the responses
            page.Response += async (sender, e) =>
            {

                if (e.Response.Url.EndsWith(productEndPoint) || e.Response.Url.EndsWith(browseEndPoint))
                {
                    try
                    {
                        var responseData = await e.Response.TextAsync();
                        if (responses.ContainsKey(e.Response.Url))
                        {
                            responses[e.Response.Url].SetResult(responseData);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
            };

            // Retrieve and print the responses
            foreach (var response in responses)
            {
                var responseData = await response.Value.Task;

                if (response.Key.EndsWith(productEndPoint))
                {
                    var productData = JsonSerializer.Deserialize<ProductAPIModel>(responseData);

                    for (int i = 0; i < productData.Products.Count; i++)
                    {
                        Console.WriteLine($"Position: {i + 1}  Title: {productData.Products[i].Name}");
                    }
                }
                else if (response.Key.EndsWith(browseEndPoint))
                {
                    var browseData = JsonSerializer.Deserialize<BrowseAPIModel>(responseData);

                    for (int i = 0; i < browseData.Bundles.Count; i++)
                    {
                        Console.WriteLine($"Position: {i + 1}  Title: {browseData.Bundles[i].Name}");
                    }
                }
                else
                {
                    Console.WriteLine($"API Response from URL {response.Key}:");
                    Console.WriteLine(responseData);
                }

                //Console.WriteLine($"API Response from URL {response.Key}:");
                //Console.WriteLine(responseData);
            }

            //await ClearCookiesAndCacheAsync(page);
            await ClearAllStorageAsync(page);

            await Task.Delay(300);

            await page.CloseAsync();

            await Task.Delay(300);

            // Close the browser
            await browser.CloseAsync();
        }

        //private static async Task ClearCookiesAndCacheAsync(IPage page)
        //{
        //    var client = page.Client;

        //    // Clear cookies
        //    var cookies = await page.GetCookiesAsync();
        //    foreach (var cookie in cookies)
        //    {
        //        await page.DeleteCookieAsync(cookie);
        //    }

        //    // Clear browser cache
        //    await client.SendAsync("Network.clearBrowserCache");
        //}



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
    }   

    
}


