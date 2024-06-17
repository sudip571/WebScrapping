// final common code


using PuppeteerSharp;
using PuppeteerSharp.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebScraping.BigW.Models;
using WebScraping.Bunnings.Models;
using WebScraping.Danmurphy.Models;
using WebScraping.FirstChoiceLiquor.Models;
using WebScraping.PetStock.Models;
using WebScraping.WoolWorths.Models;

namespace WebScraping
{
    public class Crawler
    {
        
        public static async Task<List<T>> GetCrawledData<T>(RetailerSiteDetails retailerSiteDetails)
        {
            List<T> responseDetails = new();
            int processId;
            
            try
            {
                var responseTasks = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = retailerSiteDetails.IsHeadLess,
                    //ExecutablePath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"
                });
                processId = browser.Process.Id;
                

                var page = await browser.NewPageAsync();

                await page.SetRequestInterceptionAsync(true);
                page.Request += async (sender, e) =>
                {
                    if (e.Request.Url.EndsWithAny(retailerSiteDetails.APIEndPoint, retailerSiteDetails.CheckType, retailerSiteDetails.Othervalues) && e.Request.Method == retailerSiteDetails.HttpMethod)
                    {
                        // Extract request details
                        var headers = e.Request.Headers;
                        var postData = e.Request.PostData != null ? e.Request.PostData.ToString() : "";

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
                        // Create Task
                        var key = Guid.NewGuid().ToString();
                        var tcs = new TaskCompletionSource<string>();
                        responseTasks[key] = tcs;

                        _ = Task.Run(() => ProcessRequestsInBackground<T>(initialRequestDetails, responseTasks, responseDetails, retailerSiteDetails, key));

                        // Continue the Puppeteer request
                        await e.Request.ContinueAsync();

                    }
                    else
                    {
                        // Continue the request without modification
                        await e.Request.ContinueAsync();
                    }
                };

                await page.GoToAsync(retailerSiteDetails.PageUrl, timeout: retailerSiteDetails.TimeOut, new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle2 });

                // Wait for all background tasks to complete
                await Task.WhenAll(responseTasks.Values.Select(tcs => tcs.Task));

                await ClearAllStorageAsync(page);

                await Task.Delay(300);

                await page.CloseAsync();

                await Task.Delay(300);

                await browser.CloseAsync();

                try
                {
                    Process browserProcess = Process.GetProcessById(processId);
                    if (browserProcess != null && !browserProcess.HasExited)
                    {
                        browserProcess.Kill();
                        Console.WriteLine($"Browser process with ID {browserProcess.Id} has been killed.");
                    }
                }
                catch (Exception ex)
                {
                  //
                }
                

                return responseDetails;
            }
            catch (Exception ex)
            {
                //TODO
                throw;
            }
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
        private static string ModifyRequestData(string originalPostData, string keyword, int pageNumber, int pageSize, string website = "")
        {
            string pattern1 = @"""pageSize"":\d+";
            string pattern2 = @"""PageSize"":\d+";
            string pattern3 = @"""PageNumber"":\d+";
            string pattern4 = @"""pageNumber"":\d+";
            string pattern5 = @"""SearchTerm"":""[^""]*""";
            string pattern6 = @"""searchTerm"":""[^""]*""";
            string pattern7 = @"""page"":\d+";
            string pattern8 = @"""perPage"":\d+";

            string newPostData = originalPostData;


            //newPostData = Regex.Replace(newPostData, pattern1, "\"pageSize\":20");
            //newPostData = Regex.Replace(newPostData, pattern2, "\"PageSize\":20");

            newPostData = Regex.Replace(newPostData, pattern3, $"\"PageNumber\":{pageNumber}");
            newPostData = Regex.Replace(newPostData, pattern4, $"\"pageNumber\":{pageNumber}");
            newPostData = Regex.Replace(newPostData, pattern5, $"\"SearchTerm\":\"{keyword}\"");
            newPostData = Regex.Replace(newPostData, pattern6, $"\"searchTerm\":\"{keyword}\"");

            newPostData = Regex.Replace(newPostData, pattern7, $"\"page\":{pageNumber}");
            newPostData = Regex.Replace(newPostData, pattern8, $"\"perPage\":{pageSize}");

            // petstock
            string petstockPageNo = @"page=\d+";           
            string key = "query";
            string petstockKeyword = $"(?<=[&?]{key}=)[^&]*";
            string replacement = Uri.EscapeDataString(keyword);
            newPostData = Regex.Replace(newPostData, petstockPageNo, $"page={pageNumber}&hitsPerPage={pageSize}");
            newPostData = Regex.Replace(newPostData, petstockKeyword, replacement);

            // bunnings
            string bunningPageSize = @"""numberOfResults"":\d+";
            string bunningKeyword = @"""q"":""[^""]*"""; 
            newPostData = Regex.Replace(newPostData, bunningPageSize, $"\"numberOfResults\":100");
            newPostData = Regex.Replace(newPostData, bunningKeyword, $"\"q\":\"{keyword}\"");

            return newPostData;
        }
        private static string ModifyRequestQuery(string originalPostData, string keyword, int pageNumber, int pageSize, string website = "")
        {

            string newPostData = originalPostData;

            // liquorland
            string key = "q";
            string keywordPattern = $"(?<=[&?]{key}=)[^&]*";
            string replacement = Uri.EscapeDataString(keyword);
            string liquorPageNo = @"page=\d+";
            string liquorPageSize = @"show=\d+";            
            newPostData = Regex.Replace(newPostData, liquorPageNo, $"page={pageNumber}");
            newPostData = Regex.Replace(newPostData, liquorPageSize, $"show={pageSize}");
            newPostData = Regex.Replace(newPostData, keywordPattern, replacement);

            return newPostData;
        }
        private static async Task<string> SendPostRequestAsync(RequestDetails requestDetails, Dictionary<string, string> ExpectedHeaders, string modifiedPostData)
        {
            using var client = new HttpClient();

            // Set the headers
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestDetails.Url);
            foreach (var header in requestDetails.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            foreach (var header in ExpectedHeaders)
            {
                if (!requestMessage.Headers.Contains(header.Key))
                {
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
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
            var encoding = response.Content.Headers.ContentEncoding.FirstOrDefault();
            var searchedData = await DecodeResponse(encoding, response);

            //var responseContent = await response.Content.ReadAsStringAsync();
            return searchedData;
            //var responseContent = await response.Content.ReadAsStringAsync();
            //return responseContent;
        }
        private static async Task<string> SendGetRequestAsync(RequestDetails requestDetails, Dictionary<string, string> ExpectedHeaders,string url)
        {

            using var client = new HttpClient();

            // Set the headers
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            foreach (var header in requestDetails.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            foreach (var header in ExpectedHeaders)
            {
                if (!requestMessage.Headers.Contains(header.Key))
                {
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Ensure Accept-Language is set
            if (!requestMessage.Headers.Contains("Accept-Language"))
            {
                requestMessage.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            }

            // Ensure Sec-Fetch-Site is set
            if (!requestMessage.Headers.Contains("Sec-Fetch-Site"))
            {
                requestMessage.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
            }           

            if (!requestMessage.Headers.Contains("Accept-Encoding"))
            {
                requestMessage.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br, zstd");
            }

            // Set the Accept header to indicate that we expect a JSON response
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Set cookies
            if (!string.IsNullOrEmpty(requestDetails.CookieHeader))
            {
                requestMessage.Headers.Add("Cookie", requestDetails.CookieHeader);
            }           

            // Send the request and get the response
            var response = await client.SendAsync(requestMessage);
            var encoding = response.Content.Headers.ContentEncoding.FirstOrDefault();
            var searchedData = await DecodeResponse(encoding, response);

            //var responseContent = await response.Content.ReadAsStringAsync();
            return searchedData;
        }
        private static async Task ProcessRequestsInBackground<T>(RequestDetails initialRequestDetails, ConcurrentDictionary<string, TaskCompletionSource<string>> responseTasks, List<T> responseDetails, RetailerSiteDetails retailerSiteDetails,string TaskKey)
        {

            foreach (var item in retailerSiteDetails.Keywords)
            {
                for (int i = retailerSiteDetails.PageStart; responseDetails.Count < 100; i++)
                {
                    var modifiedRequestData = retailerSiteDetails.HttpMethod == HttpMethod.Post ?
                        ModifyRequestData(initialRequestDetails.PostData, item, i, retailerSiteDetails.PageSize)
                        : ModifyRequestQuery(initialRequestDetails.Url, item, i, retailerSiteDetails.PageSize);
                    //var key = $"{item}-{i}-{Guid.NewGuid()}";
                    //var tcs = new TaskCompletionSource<string>();
                    //responseTasks[key] = tcs;

                    // delay each request by 10 sec
                    await Task.Delay(10000);

                    var response = retailerSiteDetails.HttpMethod == HttpMethod.Post ? await SendPostRequestAsync(initialRequestDetails, retailerSiteDetails.ExpectedHeaders, modifiedRequestData)
                        : await SendGetRequestAsync(initialRequestDetails, retailerSiteDetails.ExpectedHeaders, modifiedRequestData);
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        if (response.IsHtml())
                        {
                            Console.WriteLine("Could not get response");
                            Console.WriteLine(response);
                            responseTasks[TaskKey].SetResult("");                            
                            break;
                        }
                        var productData = DeserializeJsonWithTotalCount<T>(response, retailerSiteDetails.WebSiteName, initialRequestDetails.Url);
                        if (productData.TotalCount == 0 || productData.SearchedData == null || productData.SearchedData.Count == 0)
                        {
                            responseTasks[TaskKey].SetResult("");
                            break;
                        }
                        responseDetails.AddRange(productData.SearchedData);

                        if (responseDetails.Count >= 100 || responseDetails.Count >= productData.TotalCount)
                        {
                            responseTasks[TaskKey].SetResult("");
                            // when you get all data, set all other task to complete
                            foreach (var rtsc in responseTasks.Values)
                            {
                                rtsc.SetResult("");
                            }
                            break;
                        }
                    }
                }
            }
            //foreach (var rtsc in responseTasks.Values)
            //{
            //    rtsc.SetResult("");
            //}
        }
        //private static async Task ProcessRequestsInBackground<T>(RequestDetails initialRequestDetails, ConcurrentDictionary<string, TaskCompletionSource<string>> responseTasks, List<T> responseDetails, RetailerSiteDetails retailerSiteDetails)
        //{

        //    foreach (var item in retailerSiteDetails.Keywords)
        //    {
        //        for (int i = retailerSiteDetails.PageStart; responseDetails.Count < 100; i++)
        //        {
        //            var modifiedRequestData = retailerSiteDetails.HttpMethod == HttpMethod.Post ? 
        //                ModifyRequestData(initialRequestDetails.PostData, item, i, retailerSiteDetails.PageSize)
        //                : ModifyRequestQuery(initialRequestDetails.Url, item, i, retailerSiteDetails.PageSize);
        //            var key = $"{item}-{i}-{Guid.NewGuid()}";
        //            var tcs = new TaskCompletionSource<string>();
        //            responseTasks[key] = tcs;

        //            // delay each request by 10 sec
        //            await Task.Delay(10000);

        //            var response = retailerSiteDetails.HttpMethod == HttpMethod.Post ? await SendPostRequestAsync(initialRequestDetails, retailerSiteDetails.ExpectedHeaders, modifiedRequestData)
        //                :  await SendGetRequestAsync(initialRequestDetails, retailerSiteDetails.ExpectedHeaders, modifiedRequestData);
        //            if (!string.IsNullOrWhiteSpace(response))
        //            {
        //                if (response.IsHtml())
        //                {
        //                    Console.WriteLine("Could not get response");
        //                    Console.WriteLine(response);
        //                    foreach (var rtsc in responseTasks.Values)
        //                    {
        //                        rtsc.SetResult("");
        //                    }
        //                    break;
        //                }
        //                var productData = DeserializeJsonWithTotalCount<T>(response, retailerSiteDetails.WebSiteName, initialRequestDetails.Url);
        //                if (productData.TotalCount == 0 || productData.SearchedData == null || productData.SearchedData.Count == 0)
        //                {
        //                    tcs.SetResult("");
        //                    break;
        //                }
        //                responseDetails.AddRange(productData.SearchedData);

        //                if (responseDetails.Count >= 100 || responseDetails.Count >= productData.TotalCount)
        //                {
        //                    foreach (var rtsc in responseTasks.Values)
        //                    {
        //                        rtsc.SetResult("");
        //                    }
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //    foreach (var rtsc in responseTasks.Values)
        //    {
        //        rtsc.SetResult("");
        //    }
        //}
        private static List<T> DeserializeJson<T>(string jsonString, string webSite, string? api = null)
        {
            if (string.Equals(webSite, "woolworth", StringComparison.OrdinalIgnoreCase))
            {
                var productData = JsonSerializer.Deserialize<WoolWorths.Models.ProductAPIModel>(jsonString);
                var productDetails = productData.Products.SelectMany(p => p.ProductDetail).ToList();
                return (List<T>)(object)productDetails;
            }
            else
            {
                return JsonSerializer.Deserialize<List<T>>(jsonString);
            }

        }
        private static (int TotalCount, List<T> SearchedData) DeserializeJsonWithTotalCount<T>(string jsonString, string webSite, string? api = null)
        {
            if (string.Equals(webSite, "woolworth", StringComparison.OrdinalIgnoreCase))
            {
                var productData = JsonSerializer.Deserialize<WoolWorths.Models.ProductAPIModel>(jsonString);
                var productDetails = productData.Products.SelectMany(p => p.ProductDetail).ToList();
                return (productData.SearchResultsCount, (List<T>)(object)productDetails);
            }
            else if (string.Equals(webSite, "bigw", StringComparison.OrdinalIgnoreCase))
            {
                var productData = JsonSerializer.Deserialize<BigWAPIModel>(jsonString);
                var productDetails = productData.Organic.Results.Where(x => x.Attributess.ListingStatus == "LISTEDSELLABLE").Select(p => new BigWResponse
                {
                    Name = p.Information.Name,
                    SKUID = p.Identifiers.ArticleId
                }).ToList();

                return (productData.Organic.ResultsCount, (List<T>)(object)productDetails);
            }
            
            else if (string.Equals(webSite, "firstchoiceliquor", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(webSite, "liquoreland", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(webSite, "vintage", StringComparison.OrdinalIgnoreCase))
            {
                var productData = JsonSerializer.Deserialize<FirstChoiceLiquorAPIModel>(jsonString);
                var productDetails = productData.Products.Select(p => new FirstChoiceLiquorResponse
                {
                    Name = p.Name,
                    SKUID = p.Id
                }).ToList();

                return (productData.Meta.Page.ProductCount, (List<T>)(object)productDetails);
            }
            
            else if (string.Equals(webSite, "danmurphy", StringComparison.OrdinalIgnoreCase))
            {
                if (api.Contains("/apis/ui/Search/products"))
                {
                    var productData = JsonSerializer.Deserialize<Danmurphy.Models.ProductAPIModel>(jsonString);
                    if (productData is null)
                    {
                        return (0,null);
                    }
                    var productDetails = productData.Products.Select(p => new DanMurphyResponse
                    {
                        Name = p.Name,
                        SKUID = p.PackDefaultStockCode
                    }).ToList();

                    return (productData.SearchResultsCount, (List<T>)(object)productDetails);
                }
                else if (api.Contains("/apis/ui/Browse"))
                {
                    var productData = JsonSerializer.Deserialize<BrowseAPIModel>(jsonString);
                    var productDetails = productData.Bundles.Select(p => new DanMurphyResponse
                    {
                        Name = p.Name,
                        SKUID = p.PackDefaultStockCode
                    }).ToList();

                    return (productData.TotalRecordCount, (List<T>)(object)productDetails);
                }
                else if (api.Contains("/apis/ui/ProductGroup/Products"))
                {
                    var productData = JsonSerializer.Deserialize<ProductGroupAPIModel>(jsonString);
                    var productDetails = productData.Items.Select(p => new DanMurphyResponse
                    {
                        Name = p.Name,
                        SKUID = p.PackDefaultStockCode
                    }).ToList();

                    return (productData.TotalRecordCount, (List<T>)(object)productDetails);
                }
                return new(0, null);
                
            }

            else if (string.Equals(webSite, "bunnings", StringComparison.OrdinalIgnoreCase))
            {
                var productData = JsonSerializer.Deserialize<BunningAPIModel>(jsonString);
                var productDetails = productData.Data.Results.Select(p => new BunningResponse
                {
                    Name = p.Raw.Name,
                    SKUID = p.Raw.ItemNumber
                }).ToList();

                return (productData.Data.TotalCount, (List<T>)(object)productDetails);
            } 
            
            else if (string.Equals(webSite, "petstock", StringComparison.OrdinalIgnoreCase))
            {
                var productData = JsonSerializer.Deserialize<PetStockAPIModel>(jsonString);
                var productDetails = productData.Results.FirstOrDefault()?.Hits.Select(p => new ResponseDetails
                {
                    Name = p.Title,
                    SKUID = p.SKU
                }).ToList();

                return (productData.Results.FirstOrDefault()?.TotalCount ?? 0, (List<T>)(object)productDetails);
            }
            else
            {
                return (0, JsonSerializer.Deserialize<List<T>>(jsonString));
            }

        }
        private static async Task<string> DecodeResponse(string encoding, HttpResponseMessage response )
        {
            if (encoding == "gzip")
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                using (var reader = new StreamReader(decompressedStream, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            else if (encoding == "deflate")
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                using (var decompressedStream = new DeflateStream(responseStream, CompressionMode.Decompress))
                using (var reader = new StreamReader(decompressedStream, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            else if (encoding == "br")
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                using (var decompressedStream = new BrotliStream(responseStream, CompressionMode.Decompress))
                using (var reader = new StreamReader(decompressedStream, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            //else if (encoding == "zstd")
            //{
            //    // Handle zstd compression
            //    // You need to use a library that supports zstd decompression, as it's not available in .NET Framework by default
            //    // Example: https://github.com/ari-zz/zstdnet
            //    // Implement the zstd decompression logic here
            //    // This example assumes you have a method called DecompressZstd that returns the decompressed content
            //    // Replace the placeholder logic with your actual implementation
            //    using (var responseStream = await response.Content.ReadAsStreamAsync())
            //    {
            //        byte[] decompressedBytes = DecompressZstd(responseStream);
            //        return Encoding.UTF8.GetString(decompressedBytes);
            //    }
            //}
            else
            {
                // If no compression method is specified, read the response content directly
                return await response.Content.ReadAsStringAsync();
            }
        }

        

    }
    public static class StringExtensions
    {
        public static bool EndsWithAny(this string input, List<string> values, string checkType = "", List<string> otherValue = null)
        {
            bool result = false;
            if (string.IsNullOrEmpty(input) || values == null || !values.Any())
            {
                return result;
            }
            if (!string.IsNullOrWhiteSpace(checkType))
            {
                result = values.Any(value => input.Contains(value, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                result = values.Any(value => input.EndsWith(value, StringComparison.OrdinalIgnoreCase));
            }
            if (otherValue is not null && otherValue.Count > 0)
            {
                result = otherValue.Any(value => input.Contains(value, StringComparison.OrdinalIgnoreCase));
            }
            return result;
        }
        public static bool IsValidJson(this string input)
        {
            input = input.Trim();
            if ((input.StartsWith("{") && input.EndsWith("}")) || // For object
                (input.StartsWith("[") && input.EndsWith("]")))   // For array
            {
                try
                {
                    var obj = JsonDocument.Parse(input);
                    return true;
                }
                catch (JsonException)
                {
                    // Exception indicates it is not a valid JSON
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool IsHtml(this string input)
        {
            input = input.Trim();
            if (input.StartsWith("<") && input.EndsWith(">"))
            {
                // Basic check for common HTML tags
                return input.Contains("<html") || input.Contains("<head") || input.Contains("<body") || input.Contains("<div") || input.Contains("<span") || input.Contains("<script");
            }
            return false;
        }
    }
}
