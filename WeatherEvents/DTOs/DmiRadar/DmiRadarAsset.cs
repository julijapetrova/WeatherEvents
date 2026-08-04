using System.Text.Json.Serialization;

namespace WeatherEvents.DTOs.DmiRadar
{
    public class DmiRadarAsset
    {
        [JsonPropertyName("data")]
        public RadarDataResource? Data { get; set; }
    }

    public class RadarDataResource
    {
        [JsonPropertyName("type")]
        public string? MediaType { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("roles")]
        public List<string>? Roles { get; set; }

        [JsonPropertyName("href")]
        public string Href { get; set; } = string.Empty;
    }
}