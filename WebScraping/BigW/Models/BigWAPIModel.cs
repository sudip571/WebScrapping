using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebScraping.BigW.Models
{
    public class BigWAPIModel
    {
        [JsonPropertyName("organic")]
        public Organic Organic { get; set; }
       
    }
    public class Organic
    {
        [JsonPropertyName("results")]
        public List<Result> Results { get; set; }

        [JsonPropertyName("resultsCount")]
        public int ResultsCount { get; set; }
    }

    public class Result
    {
        [JsonPropertyName("identifiers")]
        public Identifiers Identifiers { get; set; }

        [JsonPropertyName("information")]
        public Information Information { get; set; }
        
        [JsonPropertyName("attributes")]
        public Attributess Attributess { get; set; }


    }
    public class Identifiers
    {
        [JsonPropertyName("articleId")]
        public string ArticleId { get; set; }
    }
    public class Information
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
    public class Attributess
    {
        [JsonPropertyName("listingStatus")]
        public string ListingStatus { get; set; } // LISTEDSELLABLE
    }
    public class BigWResponse
    {
        public string SKUID { get; set; }
        public string Name { get; set; }
    }
}


// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);






