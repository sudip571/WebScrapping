using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using WebScraping.Bunnings.Models;

namespace WebScraping.Bunnings
{
    public class CrawlBunning
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
                PageSize = 100,
                //CheckType = "contains"
            };

            var data = await Crawler.GetCrawledData<BunningResponse>(retailerSiteDetails);
            int count = 1;
            foreach (var item in data.Take(100))
            {
                Console.WriteLine($"SN: {count} SKUID: {item.SKUID}  Title: {item.Name}");
                count++;
            }
        }

        public static async Task GetBunningPredefinedRequest(string keyword)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.prod.bunnings.com.au/v1/coveo/search");
            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
            request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
            request.Headers.Add("Authorization", "Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IkJGRTFEMDBBRUZERkVDNzM4N0E1RUFFMzkxNjRFM0MwMUJBNzVDODciLCJ4NXQiOiJ2LUhRQ3VfZjdIT0hwZXJqa1dUandCdW5YSWMiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2J1bm5pbmdzLmNvbS5hdS8iLCJuYmYiOjE3MTgzNTMwNDUsImlhdCI6MTcxODM1MzA0NSwiZXhwIjoxNzE4Nzg1MDQ1LCJhdWQiOlsiQ2hlY2tvdXQtQXBpIiwiY3VzdG9tZXJfYnVubmluZ3MiLCJodHRwczovL2J1bm5pbmdzLmNvbS5hdS9yZXNvdXJjZXMiXSwic2NvcGUiOlsiY2hrOmV4ZWMiLCJjbTphY2Nlc3MiLCJlY29tOmFjY2VzcyIsImNoazpwdWIiXSwiYW1yIjpbImV4dGVybmFsIl0sImNsaWVudF9pZCI6ImJ1ZHBfZ3Vlc3RfdXNlcl9hdSIsInN1YiI6IjkwNzZkNjVlLTA0M2ItNDYwMS1hZmEwLWFiMzczZWNkYTc1ZCIsImF1dGhfdGltZSI6MTcxODM1MzA0NSwiaWRwIjoibG9jYWxsb29wYmFjayIsImItaWQiOiI5MDc2ZDY1ZS0wNDNiLTQ2MDEtYWZhMC1hYjM3M2VjZGE3NWQiLCJiLXJvbGUiOiJndWVzdCIsImItdHlwZSI6Imd1ZXN0IiwibG9jYWxlIjoiZW5fQVUiLCJiLWNvdW50cnkiOiJBVSIsImFjdGl2YXRpb25fc3RhdHVzIjoiRmFsc2UiLCJ1c2VyX25hbWUiOiI5MDc2ZDY1ZS0wNDNiLTQ2MDEtYWZhMC1hYjM3M2VjZGE3NWQiLCJiLXJiYWMiOlt7ImFzYyI6IjI1ODEyYTVmLTk2NzAtNDkzNC04NDMzLWMwN2Q4ODg1MGJjOSIsInR5cGUiOiJDIiwicm9sIjpbIkNISzpHdWVzdCJdfV0sInNpZCI6IjVGRjAxM0Y2QjZFODU0ODk2Q0QzRjlGNDI3MUVDMDAyIiwianRpIjoiMzVDNzQ0NkZFNzkwMEI2MTg0ODU5N0EyNTcyQkVENjEifQ.viIkhglTGQOIqxAgi8uBA7rYJ3FRlFOqLl60fupP86KZX-5WQ8x7OSiLZPN2ut9_qoL-lq9PueDH9Ynb3SAwyeqtc5ttTfYjBdVmsTG6Ct_DzNIw4L1l6D2PA8JCnApEdOeyZbgS9X_N9f0G94UcjhnErJQJrarqklt3CsmfJ6vujjDHEvT9uNdSzY2WP23tgrS8bsPExxBxhGTU54CDpXobdzZbe-L3pdp8_q8FksSTD58dQLRyqTECwEOh4HHK9r_5Zz8pKBZFuEe4vtF8qt1gJrFpv7jLvTXceWDYmCGUc__XFpOR2VbTD_Qv3RTMQHElrZC0ggEilP75rClMMg");
            request.Headers.Add("Referer", "https://www.bunnings.com.au/");
            request.Headers.Add("Sec-Fetch-Mode", "cors");
            request.Headers.Add("Sec-Fetch-Site", "same-site");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0");
            request.Headers.Add("X-region", "VICMetro");
            request.Headers.Add("clientId", "mHPVWnzuBkrW7rmt56XGwKkb5Gp9BJMk");
            request.Headers.Add("correlationid", "11747720-218f-11ef-af6d-8513fa6052ff");
            request.Headers.Add("country", "AU");
            request.Headers.Add("currency", "AUD");
            request.Headers.Add("locale", "en_AU");
            request.Headers.Add("locationCode", "6400");
            request.Headers.Add("sec-ch-ua", "\"Microsoft Edge\";v=\"125\", \"Chromium\";v=\"125\", \"Not.A/Brand\";v=\"24\"");
            request.Headers.Add("sec-ch-ua-mobile", "?0");
            request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.Add("sessionid", "f09da2b0-218e-11ef-a406-418eaecb0e16");
            request.Headers.Add("stream", "RETAIL");
            
            string jsonData = @"{""debug"":false,""enableDidYouMean"":true,""enableDuplicateFiltering"":false,""enableQuerySyntax"":false,""facetOptions"":{""freezeFacetOrder"":true},""filterField"":""@baseid"",""filterFieldRange"":10,""lowerCaseOperators"":true,""partialMatch"":true,""partialMatchKeywords"":2,""partialMatchThreshold"":""30%"",""questionMark"":true,""enableWordCompletion"":true,""firstResult"":0,""isGuestUser"":true,""numberOfResults"":""100"",""sortCriteria"":""relevancy"",""analytics"":{""clientId"":""00e44771-264d-4fad-a429-39bab9b14f4a""},""aq"":""@availableinregions==(vicmetro) AND @price_6400 > 0 AND @isactive==true AND @batchcountry==(AU)"",""context"":{""website"":""AU""},""facets"":[{""facetId"":""supercategories"",""field"":""supercategories"",""type"":""specific"",""injectionDepth"":1000,""delimitingCharacter"":""|"",""filterFacetCount"":true,""basePath"":[],""filterByBasePath"":false,""currentValues"":[],""preventAutoSelect"":false,""numberOfValues"":200,""isFieldExpanded"":false},{""facetId"":""@productranges_vicmetro"",""field"":""productranges_vicmetro"",""type"":""specific"",""injectionDepth"":1000,""filterFacetCount"":true,""currentValues"":[],""numberOfValues"":8,""freezeCurrentValues"":false,""preventAutoSelect"":false,""isFieldExpanded"":false},{""facetId"":""@price_6400"",""field"":""price_6400"",""type"":""numericalRange"",""injectionDepth"":1000,""filterFacetCount"":true,""preventAutoSelect"":false,""currentValues"":[],""numberOfValues"":4,""freezeCurrentValues"":false,""generateAutomaticRanges"":true,""rangeAlgorithm"":""even""},{""facetId"":""@brandname"",""field"":""brandname"",""type"":""specific"",""injectionDepth"":4000,""filterFacetCount"":true,""currentValues"":[],""numberOfValues"":50,""freezeCurrentValues"":false,""preventAutoSelect"":false,""isFieldExpanded"":false},{""facetId"":""@rating"",""field"":""rating"",""type"":""numericalRange"",""injectionDepth"":1000,""filterFacetCount"":true,""preventAutoSelect"":false,""currentValues"":[{""start"":1,""end"":1.99,""endInclusive"":true,""state"":""idle"",""preventAutoSelect"":false},{""start"":2,""end"":2.99,""endInclusive"":true,""state"":""idle"",""preventAutoSelect"":false},{""start"":3,""end"":3.99,""endInclusive"":true,""state"":""idle"",""preventAutoSelect"":false},{""start"":4,""end"":4.99,""endInclusive"":true,""state"":""idle"",""preventAutoSelect"":false},{""start"":5,""end"":5.99,""endInclusive"":true,""state"":""idle"",""preventAutoSelect"":false}],""numberOfValues"":5,""freezeCurrentValues"":false},{""facetId"":""familycolourname"",""field"":""familycolourname"",""type"":""specific"",""injectionDepth"":1000,""delimitingCharacter"":""|"",""filterFacetCount"":true,""basePath"":[],""filterByBasePath"":false,""currentValues"":[],""numberOfValues"":200,""preventAutoSelect"":false,""isFieldExpanded"":false},{""facetId"":""@height"",""field"":""height"",""type"":""numericalRange"",""injectionDepth"":1000,""filterFacetCount"":true,""preventAutoSelect"":false,""currentValues"":[],""numberOfValues"":8,""freezeCurrentValues"":false,""generateAutomaticRanges"":true,""rangeAlgorithm"":""even""},{""facetId"":""@width"",""field"":""width"",""type"":""numericalRange"",""injectionDepth"":1000,""filterFacetCount"":true,""preventAutoSelect"":false,""currentValues"":[],""numberOfValues"":8,""freezeCurrentValues"":false,""generateAutomaticRanges"":true,""rangeAlgorithm"":""even""},{""facetId"":""@depth"",""field"":""depth"",""type"":""numericalRange"",""injectionDepth"":1000,""filterFacetCount"":true,""preventAutoSelect"":false,""currentValues"":[],""numberOfValues"":8,""freezeCurrentValues"":false,""generateAutomaticRanges"":true,""rangeAlgorithm"":""even""},{""facetId"":""@volume"",""field"":""volume"",""type"":""numericalRange"",""injectionDepth"":1000,""filterFacetCount"":true,""preventAutoSelect"":false,""currentValues"":[],""numberOfValues"":8,""freezeCurrentValues"":false,""generateAutomaticRanges"":true,""rangeAlgorithm"":""even""},{""facetId"":""@weight"",""field"":""weight"",""type"":""numericalRange"",""injectionDepth"":1000,""filterFacetCount"":true,""preventAutoSelect"":false,""currentValues"":[],""numberOfValues"":8,""freezeCurrentValues"":false,""generateAutomaticRanges"":true,""rangeAlgorithm"":""even""}],""groupBy"":[{""constantQueryOverride"":""@source==(PRODUCT_STREAM_AU)"",""field"":""@price_6400"",""generateAutomaticRanges"":true,""maximumNumberOfValues"":1,""advancedQueryOverride"":""@uri"",""queryOverride"":""drainage channel""},{""advancedQueryOverride"":""@uri"",""field"":""@supercategoriesurl""},{""advancedQueryOverride"":"""",""constantQueryOverride"":""@availableinregions==(vicmetro) AND @price_6400 > 0 AND @isactive==true AND @batchcountry==(AU) AND @source==(PRODUCT_STREAM_AU) OR @z95xtemplate==(d16262d2fbdc4dbfbb6443c5c4ebc3db) AND @showInSearch=1 AND @isServiceStatusActive=1 AND @source==BUDP_AU_web_index-ProdEnv_BunningsSC OR @z95xtemplate==(4cb3a49604b24ef1b674576fd8f4650b,cf6199d71fa0444eaa247f722d26549e,8b6affd59ae942239d81d0850d888b6e,5a2d6c7ccfe54199976037394fdf1951,36815b6e0e7447fbbb5e968469c5c722,faeb71ebfdff45e49fde2659dd1bfa0f,45fc1c8036984dd6bcd583d87478a544) AND @showInSearch=1 AND @source==BUDP_AU_web_index-ProdEnv_BunningsSC OR @source==(DOCUMENTS_AU) OR @source==(BUDP_AU_web_index-ProdEnv_BunningsSC) AND @z95xtemplate==(77a66eda8b7f473aa926d50a8f275878,90e723f5117a4b5389e7f3d953107314,a15f4d221ffd48689a230c56a8af469f,d4c502cf433a48a19d2dc948e122a18e,ca541b39ae8944319af40212f45258ca,8b6affd59ae942239d81d0850d888b6e,fbb9f0d878284d1cb178655782f72107,426714719f734a9983f1b7a8703c98e8,4208175b8c98404c924bd2c8558ca279) AND @showInSearch=1 OR @source==(Brands_web_index-ProdEnv_BunningsSC) AND @z95xtemplate==(d3221b08e0a64d4aa8ac7e5a7d0ddba7) AND @language==en AND @brandActiveInCurrentLanguage=1 AND @showInSearch=1"",""field"":""@searchtab"",""filterFacetCount"":true,""queryOverride"":""drainage channel""}],""searchHub"":""PRODUCT_SEARCH"",""visitorId"":""00e44771-264d-4fad-a429-39bab9b14f4a"",""cq"":""@source==(PRODUCT_STREAM_AU)"",""fieldsToInclude"":[""source"",""thumbnailimageurl"",""supercategoriescode"",""validityscore"",""supercategoriesurl"",""supercategories"",""ratingcount"",""brandiconurl"",""transactionid"",""title"",""date"",""objecttype"",""productdimensiondepth"",""fsc"",""currency"",""moreoptions"",""colorcount"",""trustedseller"",""colornames"",""price_6400"",""rowid"",""rating"",""size"",""stockstatus"",""forhire"",""orderingid"",""baseproduct"",""bestseller"",""productroutingurl"",""brandcode"",""categories"",""productdimensionwidth"",""productdimensionheight"",""colorhexcodes"",""brandname"",""name"",""itemnumber"",""url"",""baseid"",""newarrival"",""imageurl"",""categoryiconurls"",""uri"",""availability"",""code"",""basicbundle"",""price"",""productRanges_6400"",""selectedcolorhexcode"",""description"",""keysellingpoints"",""brandurl"",""sizecount"",""timecount"",""weightcount"",""variantcount"",""volumncount"",""productRanges_vicmetro"",""stockindicator"",""productcount"",""familycolourname"",""unitofprice"",""storeattributes_6400"",""bundleproductreferences"",""isactive"",""sellericonurl"",""agerestricted"",""bundleaction"",""sellername"",""tintscount"",""tints"",""basecolor"",""volume"",""cprice_6400"",""comparisonunit"",""comparisonunitofmeasure"",""comparisonunitofmeasurecode"",""promotionalcampaign"",""promotionalcampaignstart"",""promotionalcampaignend"",""defaultofferid"",""isdeliveryincluded""],""generateAutomaticFacets"":{""desiredCount"":10,""fields"":{},""numberOfValues"":10},""pipeline"":""Variant_Product"",""q"":""mykeyword""}";
            jsonData = jsonData.Replace("mykeyword", keyword);


            var content = new StringContent(jsonData, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            var encoding = response.Content.Headers.ContentEncoding.FirstOrDefault();
            var searchedData = await DecodeResponse(encoding, response);

            var productData = JsonSerializer.Deserialize<BunningAPIModel>(searchedData);
            var productDetails = productData.Data.Results.Select(p => new BunningResponse
            {
                Name = p.Raw.Name,
                SKUID = p.Raw.ItemNumber
            }).ToList();
           
            int count = 1;
            foreach (var item in productDetails.Take(100))
            {
                Console.WriteLine($"SN: {count} SKUID: {item.SKUID}  Title: {item.Name}");
                count++;
            }
        }
        private static async Task<string> DecodeResponse(string encoding, HttpResponseMessage response)
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
}
