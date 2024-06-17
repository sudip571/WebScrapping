using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebScraping.WoolWorths.Models
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
    public class ProductAPIModel
    {
        [JsonPropertyName("Products")]
        public List<ProductsModel> Products { get; set; }
        [JsonPropertyName("SearchResultsCount")]
        public int SearchResultsCount { get; set; }
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
    public class ProductsModel
    {
        [JsonPropertyName("Products")]
        public List<ProductDetail> ProductDetail { get; set; }
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
    public class ProductDetail
    {
        [JsonPropertyName("DisplayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("Stockcode")]
        public int SKUID { get; set; }
    }
}
