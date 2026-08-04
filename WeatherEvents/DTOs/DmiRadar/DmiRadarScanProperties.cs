using System.Text.Json.Serialization;

namespace WeatherEvents.DTOs.DmiRadar
{
    public class DmiRadarScanProperties
    {
        [JsonPropertyName("datetime")]
        public DateTime Datetime { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("stationId")]
        public string StationId { get; set; } = string.Empty;
    }
}