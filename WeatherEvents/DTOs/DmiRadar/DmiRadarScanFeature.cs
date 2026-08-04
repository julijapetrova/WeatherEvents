using System.Text.Json;
using System.Text.Json.Serialization;

namespace WeatherEvents.DTOs.DmiRadar
{
    public class DmiRadarScanFeature
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "Feature";

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("geometry")]
        public DmiRadarGeometry? Geometry { get; set; }

        [JsonPropertyName("bbox")]
        public double[]? Bbox { get; set; }

        [JsonPropertyName("properties")]
        public DmiRadarScanProperties Properties { get; set; } = new();

        [JsonPropertyName("asset")]
        public DmiRadarAsset? Asset { get; set; }

        [JsonPropertyName("stac_version")]
        public string? StacVersion { get; set; }

        [JsonPropertyName("collection")]
        public string? CollectionName { get; set; }

        /// <summary>
        /// Convenience: the download URL for the HDF5 file
        /// </summary>
        [JsonIgnore]
        public string? DownloadUrl => Asset?.Data?.Href;
    }
}