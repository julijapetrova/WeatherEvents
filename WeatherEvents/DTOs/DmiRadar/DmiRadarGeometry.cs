using System.Text.Json;
using System.Text.Json.Serialization;

namespace WeatherEvents.DTOs.DmiRadar
{
    /// <summary>
    /// The bounding box of the radar scan coverage.
    /// This is what tells you which lat/lon points this scan can answer.
    /// </summary>
    public class DmiRadarGeometry
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Bounding box: [minLon, minLat, maxLon, maxLat]
        /// </summary>
        [JsonPropertyName("bbox")]
        public double[]? Bbox { get; set; }

        /// <summary>
        /// Polygon coordinates (for STAC compliance)
        /// </summary>
        [JsonPropertyName("coordinates")]
        public JsonElement? Coordinates { get; set; }
    }

}
