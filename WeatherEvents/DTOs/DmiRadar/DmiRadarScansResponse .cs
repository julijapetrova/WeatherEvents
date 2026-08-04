using System.Text.Json;
using System.Text.Json.Serialization;

namespace WeatherEvents.DTOs.DmiRadar
{
    public class DmiRadarScansResponse
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "FeatureCollection";

        [JsonPropertyName("features")]
        public List<DmiRadarScanFeature> Features { get; set; } = new();

        [JsonPropertyName("links")]
        public List<LinkItem>? Links { get; set; }  // ← Changed to LIST of LinkItem

        [JsonPropertyName("timeStamp")]
        public DateTime? Timestamp { get; set; }

        [JsonPropertyName("numberReturned")]
        public int NumberReturned { get; set; }
    }

    public class LinkItem
    {
        [JsonPropertyName("href")]
        public string Href { get; set; } = string.Empty;

        [JsonPropertyName("rel")]
        public string? Rel { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }
}