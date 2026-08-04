using System.Text.Json;
using System.Text.Json.Serialization;

namespace WeatherEvents.DTOs.DmiRadar
{
    public class DmiRadarPagingLinks
    {
        [JsonPropertyName("self")]
        public string? Self { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("prev")]
        public string? Prev { get; set; }

        [JsonPropertyName("collection")]
        public string? Collection { get; set; }

        // Allow other properties without throwing errors
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
    }
}