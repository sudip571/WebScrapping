
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
using System.Text;
using PuppeteerSharp.Input;
using System.Security.AccessControl;

namespace WebScraping.WoolWorths
{
    public class CrawlWoolWorthV2
    {
        public static async Task GetData(string keyword ="noodles")
        {
            var productEndPoint = @"/apis/ui/Search/products";
            var pageUrl = $@"https://www.woolworths.com.au/shop/search/products?searchTerm={keyword}";
            var responseTasks = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
            var searchedData = new List<ProductDetail>();

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();


            await page.SetRequestInterceptionAsync(true);

            page.Request += async (sender, e) =>
            {
                if (e.Request.Url.EndsWith(productEndPoint) && e.Request.Method == HttpMethod.Post)
                {
                    // Extract request details
                    var headers = e.Request.Headers;
                    var postData = e.Request.PostData.ToString();

                    // Get cookies
                    var cookies = await page.GetCookiesAsync(e.Request.Url);
                    var cookieHeader = string.Join("; ", cookies.Select(c => $"{c.Name}={c.Value}"));


                    //// Set up the HttpClient
                    //using var client = new HttpClient();

                    //// Set the headers
                    //var requestMessage = new HttpRequestMessage(HttpMethod.Post, e.Request.Url);
                    //foreach (var header in headers)
                    //{
                    //    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    //}

                    //// Set cookies
                    //if (!string.IsNullOrEmpty(cookieHeader))
                    //{
                    //    requestMessage.Headers.Add("Cookie", cookieHeader);
                    //}
                   
                    //// Set the payload
                    //if (!string.IsNullOrEmpty(postData))
                    //{
                    //    requestMessage.Content = new StringContent(postData, Encoding.UTF8, "application/json");
                    //}

                    //// Send the request and get the response
                    //var response = await client.SendAsync(requestMessage);
                    //var responseContent = await response.Content.ReadAsStringAsync();

                    //// Output the response for debugging purposes
                    //Console.WriteLine("Response:");
                    //Console.WriteLine(responseContent);



                    // Store initial request details
                    var initialRequestDetails = new RequestDetails
                    {
                        Url = e.Request.Url,
                        Headers = headers,
                        CookieHeader = cookieHeader,
                        PostData = postData
                    };

                   

                    _ = Task.Run(() => ProcessRequestsInBackground(initialRequestDetails, responseTasks, searchedData));

                    // Continue the Puppeteer request
                    await e.Request.ContinueAsync();

                }
                else
                {
                    // Continue the request without modification
                    await e.Request.ContinueAsync();
                }
            };

            await page.GoToAsync(pageUrl, timeout: 1200000, new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle2 });

            // Wait for all background tasks to complete
            await Task.WhenAll(responseTasks.Values.Select(tcs => tcs.Task));

            int count = 1;
            foreach (var task in responseTasks)
            {
                if (task.Value.Task.IsCompletedSuccessfully)
                {
                    foreach (var item in searchedData)
                    {
                        Console.WriteLine($"Position: {count} SKUID: {item.SKUID}  Title: {item.DisplayName}");
                        count++;
                    }
                    //var tempKeys = task.Key.Split('-');

                    //tempResultModels.Add(new TempResultModel
                    //{
                    //    APIType = tempKeys[2],
                    //    PageNo = int.Parse(tempKeys[1].Trim()),
                    //    Data = task.Value.Task.Result,
                    //    Keyword = tempKeys[0].Trim(),
                    //});
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

        private static async Task ProcessRequestsInBackground(RequestDetails initialRequestDetails, ConcurrentDictionary<string, TaskCompletionSource<string>> responseTasks, List<ProductDetail> searchedData)
        {
            List<string> Keywords = new List<string> { "fruits" };
            foreach (var item in Keywords)
            {
                for (int i = 1; searchedData.Count < 100 ; i++)
                {
                    var modifiedPostData = ModifyRequestData(initialRequestDetails.PostData, item, i);
                    var key = $"{item}-{i}";
                    var tcs = new TaskCompletionSource<string>();
                    responseTasks[key] = tcs;

                    //var response = await SendRequestAsync(initialRequestDetails, initialRequestDetails.PostData);
                    var response = await SendRequestAsync(initialRequestDetails, modifiedPostData);
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        var productData = JsonSerializer.Deserialize<ProductAPIModel>(response);
                        var productDetails = productData.Products.SelectMany(p => p.ProductDetail).ToList();
                        searchedData.AddRange(productDetails);

                        if (searchedData.Count == 100 || productData.SearchResultsCount <= 100)
                        {
                            tcs.SetResult(response);
                            break;
                        }
                    }
                   // tcs.SetResult(response);
                }
            }
            foreach (var rtsc in responseTasks.Values)
            {
                rtsc.SetResult("");
            }
            
            //var modifiedPostData = ModifyRequestData(initialRequestDetails.PostData, keyword, pageNumber);

        }
        private static string ModifyRequestData(string originalPostData, string keyword, int pageNumber)
        {
            string pattern1 = @"""pageSize"":\d+";
            string pattern2 = @"""PageSize"":\d+";
            string pattern3 = @"""PageNumber"":\d+";
            string pattern4 = @"""pageNumber"":\d+";
            string pattern5 = @"""SearchTerm"":""[^""]*""";

            string newPostData = originalPostData;


            //newPostData = Regex.Replace(newPostData, pattern1, "\"pageSize\":20");
            //newPostData = Regex.Replace(newPostData, pattern2, "\"PageSize\":20");

            newPostData = Regex.Replace(newPostData, pattern3, $"\"PageNumber\":{pageNumber}");
            newPostData = Regex.Replace(newPostData, pattern4, $"\"pageNumber\":{pageNumber}");
            newPostData = Regex.Replace(newPostData, pattern5, $"\"SearchTerm\":\"{keyword}\"");
            newPostData = Regex.Replace(newPostData, pattern5, $"\"searchTerm\":\"{keyword}\"");

            return newPostData;
        }
        public static async Task<string> SendRequestAsync(RequestDetails requestDetails, string modifiedPostData)
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
    }
}


