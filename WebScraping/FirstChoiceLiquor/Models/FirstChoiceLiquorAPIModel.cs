using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebScraping.FirstChoiceLiquor.Models
{    
    public class Product
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
    public class Meta
    {
        [JsonPropertyName("page")]
        public Page Page { get; set; }
    }

    public class Page
    {
        [JsonPropertyName("productCount")]
        public int ProductCount { get; set; }
    }
    public class FirstChoiceLiquorAPIModel
    {
        [JsonPropertyName("products")]
        public List<Product> Products { get; set; }

        [JsonPropertyName("meta")]
        public Meta Meta { get; set; }
    }

    public class FirstChoiceLiquorResponse
    {
        public string SKUID { get; set; }
        public string Name { get; set; }
    }

}
