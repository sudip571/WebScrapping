using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebScraping.Danmurphy.Models
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
    public class BrowseAPIModel
    {
        [JsonPropertyName("Bundles")]
        public List<BundlesModel> Bundles { get; set; }
        [JsonPropertyName("TotalRecordCount")]
        public int TotalRecordCount { get; set; }
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
    public class BundlesModel
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("PackDefaultStockCode")]
        public string PackDefaultStockCode { get; set; }
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
    public class ProductAPIModel
    {
        [JsonPropertyName("Products")]
        public List<ProductModel> Products { get; set; }
        [JsonPropertyName("SearchResultsCount")]
        public int SearchResultsCount { get; set; }
        
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
    public class ProductModel
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("PackDefaultStockCode")]
        public string PackDefaultStockCode { get; set; }
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
    public class ProductGroupAPIModel
    {
        [JsonPropertyName("Items")]
        public List<ItemsModel> Items { get; set; }
        [JsonPropertyName("TotalRecordCount")]
        public int TotalRecordCount { get; set; }
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
    public class ItemsModel
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("PackDefaultStockCode")]
        public string PackDefaultStockCode { get; set; }
    }
    public class DanMurphyResponse
    {
        public string SKUID { get; set; }
        public string Name { get; set; }
    }
}
