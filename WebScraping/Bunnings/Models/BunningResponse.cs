using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebScraping.FirstChoiceLiquor.Models;

namespace WebScraping.Bunnings.Models
{
    public class BunningResponse
    {
        public string SKUID { get; set; }
        public string Name { get; set; }
    }
    public class BunningAPIModel
    {
        [JsonPropertyName("data")]
        public BunningDataModel Data { get; set; }
        
    }
    public class BunningDataModel
    {
        [JsonPropertyName("results")]
        public List<BunningResultModel> Results { get; set; }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }
    }
    public class BunningResultModel
    {
        [JsonPropertyName("raw")]
        public BunningRawModel Raw { get; set; }
      
    }
    public class BunningRawModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("itemnumber")]
        public string ItemNumber { get; set; }
    }
}
