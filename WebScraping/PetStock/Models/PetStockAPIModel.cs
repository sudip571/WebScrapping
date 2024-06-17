using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebScraping.PetStock.Models
{
  
    public class PetStockAPIModel
    {
        [JsonPropertyName("results")]
        public List<PetStockResultModel> Results { get; set; }

    }
  
    public class PetStockResultModel
    {
        [JsonPropertyName("hits")]
        public List<PetStockHitModel> Hits { get; set; }
        [JsonPropertyName("nbHits")]
        public int TotalCount { get; set; }

    }
    public class PetStockHitModel
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("sku")]
        public string SKU { get; set; }
    }
}
