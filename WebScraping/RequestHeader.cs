using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebScraping
{
    internal class RequestHeader
    {
    }


    public class CurlHeaders
    {
        [JsonPropertyName("A-IM")]
        public string AIM { get; set; }

        [JsonPropertyName("Accept")]
        public string Accept { get; set; }

        [JsonPropertyName("Accept-Charset")]
        public string AcceptCharset { get; set; }

        [JsonPropertyName("Accept-Datetime")]
        public string AcceptDatetime { get; set; }

        [JsonPropertyName("Accept-Encoding")]
        public string AcceptEncoding { get; set; }

        [JsonPropertyName("Accept-Language")]
        public string AcceptLanguage { get; set; }

        [JsonPropertyName("Access-Control-Request-Method")]
        public string AccessControlRequestMethod { get; set; }

        [JsonPropertyName("Access-Control-Request-Headers")]
        public string AccessControlRequestHeaders { get; set; }

        [JsonPropertyName("Authorization")]
        public string Authorization { get; set; }

        [JsonPropertyName("Cache-Control")]
        public string CacheControl { get; set; }

        [JsonPropertyName("Connection")]
        public string Connection { get; set; }

        [JsonPropertyName("Content-Encoding")]
        public string ContentEncoding { get; set; }

        [JsonPropertyName("Content-Length")]
        public string ContentLength { get; set; }

        [JsonPropertyName("Content-MD5")]
        public string ContentMD5 { get; set; }

        [JsonPropertyName("Content-Type")]
        public string ContentType { get; set; }

        [JsonPropertyName("Cookie")]
        public string Cookie { get; set; }

        [JsonPropertyName("Date")]
        public string Date { get; set; }

        [JsonPropertyName("Expect")]
        public string Expect { get; set; }

        [JsonPropertyName("Forwarded")]
        public string Forwarded { get; set; }

        [JsonPropertyName("From")]
        public string From { get; set; }

        [JsonPropertyName("Host")]
        public string Host { get; set; }

        [JsonPropertyName("If-Match")]
        public string IfMatch { get; set; }

        [JsonPropertyName("If-Modified-Since")]
        public string IfModifiedSince { get; set; }

        [JsonPropertyName("If-None-Match")]
        public string IfNoneMatch { get; set; }

        [JsonPropertyName("If-Range")]
        public string IfRange { get; set; }

        [JsonPropertyName("If-Unmodified-Since")]
        public string IfUnmodifiedSince { get; set; }

        [JsonPropertyName("Max-Forwards")]
        public string MaxForwards { get; set; }

        [JsonPropertyName("Origin")]
        public string Origin { get; set; }

        [JsonPropertyName("Pragma")]
        public string Pragma { get; set; }

        [JsonPropertyName("Proxy-Authorization")]
        public string ProxyAuthorization { get; set; }

        [JsonPropertyName("Range")]
        public string Range { get; set; }

        [JsonPropertyName("Referer")]
        public string Referer { get; set; }

        [JsonPropertyName("TE")]
        public string TE { get; set; }

        [JsonPropertyName("Trailer")]
        public string Trailer { get; set; }

        [JsonPropertyName("Transfer-Encoding")]
        public string TransferEncoding { get; set; }

        [JsonPropertyName("User-Agent")]
        public string UserAgent { get; set; }

        [JsonPropertyName("Upgrade")]
        public string Upgrade { get; set; }

        [JsonPropertyName("Via")]
        public string Via { get; set; }

        [JsonPropertyName("Warning")]
        public string Warning { get; set; }

        [JsonPropertyName("DNT")]
        public string DNT { get; set; }

        [JsonPropertyName("X-Requested-With")]
        public string XRequestedWith { get; set; }

        [JsonPropertyName("X-CSRF-Token")]
        public string XCsrfToken { get; set; }

        [JsonPropertyName("X-Forwarded-For")]
        public string XForwardedFor { get; set; }

        [JsonPropertyName("X-Forwarded-Host")]
        public string XForwardedHost { get; set; }

        [JsonPropertyName("X-Forwarded-Proto")]
        public string XForwardedProto { get; set; }

        [JsonPropertyName("Front-End-Https")]
        public string FrontEndHttps { get; set; }

        [JsonPropertyName("X-Http-Method-Override")]
        public string XHttpMethodOverride { get; set; }

        [JsonPropertyName("X-ATT-DeviceId")]
        public string XATTDeviceId { get; set; }

        [JsonPropertyName("X-Wap-Profile")]
        public string XWapProfile { get; set; }

        [JsonPropertyName("Proxy-Connection")]
        public string ProxyConnection { get; set; }

        [JsonPropertyName("X-UIDH")]
        public string XUIDH { get; set; }

        [JsonPropertyName("X-Csrf-Token")]
        public string XCsrfToken2 { get; set; }

        [JsonPropertyName("X-Request-ID")]
        public string XRequestId { get; set; }

        [JsonPropertyName("X-Correlation-ID")]
        public string XCorrelationId { get; set; }

        [JsonPropertyName("Save-Data")]
        public string SaveData { get; set; }

        [JsonPropertyName("ADRUM")]
        public string ADRUM { get; set; }

        [JsonPropertyName("client-id")]
        public string ClientId { get; set; }

        [JsonPropertyName("sec-ch-ua")]
        public string SecChUa { get; set; }

        [JsonPropertyName("sec-ch-ua-mobile")]
        public string SecChUaMobile { get; set; }

        [JsonPropertyName("sec-ch-ua-platform")]
        public string SecChUaPlatform { get; set; }

        [JsonPropertyName("user-id")]
        public string UserId { get; set; }
    }

    public static class HttpRequestMessageExtensions
    {
        public static void AddHeaders(this HttpRequestMessage requestMessage, CurlHeaders headers)
        {
            var headerProperties = headers.GetType().GetProperties();

            foreach (var property in headerProperties)
            {
                var jsonPropertyNameAttribute = property.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false).FirstOrDefault() as JsonPropertyNameAttribute;
                if (jsonPropertyNameAttribute != null)
                {
                    var headerName = jsonPropertyNameAttribute.Name;
                    var headerValue = property.GetValue(headers) as string;

                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        requestMessage.Headers.TryAddWithoutValidation(headerName, headerValue);
                    }
                }
            }
        }
    }
    // Example usage
    //var headers = new CurlHeaders
    //{
    //    Accept = "application/json, text/plain, */*",
    //    AcceptEncoding = "gzip, deflate, br, zstd",
    //    AcceptLanguage = "en-US,en;q=0.9",
    //    Cookie = "__uzma=10654681-d8a8-48f5-a57d-0c0ecaa3fa58; __uzmb=1717148968; _gcl_au=1.1.1629414711.1717148970; KP_UIDz-ssn=032jHfuiuxSOQoAx0NnT7qZ0jeypMF3VXOYhLFdP8INP4T9zK1lRfFve45yYPyglsXb6eMS3k9j0h8s33o0CD8nrN5Gf9SLWWrHxOOPhjnXo7nU2VWNFaoP2X0QlIwOFlwkPPCwhzMEpAIMwN1kRyQ8BDQCEh2stq1HGYugFZr",
    //    Referer = "https://www.firstchoiceliquor.com.au/",
    //    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36",
    //    // Add other headers as needed
    //};

    //var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://www.vintagecellars.com.au/api/search/vc/nsw?q=wine&facets=&page=1&sort=&show=60");
    //requestMessage.AddHeaders(headers);
}
