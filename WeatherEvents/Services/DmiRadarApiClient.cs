using Azure.Core;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using WeatherEvents.DTOs.DmiRadar;
using WeatherEvents.Repositories;

namespace WeatherEvents.Services
{
    public class DmiRadarApiClient : IDmiRadarApiClient
    {
        private readonly ILogger<DmiRadarApiClient> _logger;
        private readonly HttpClient _httpClient;
        public DmiRadarApiClient(
              HttpClient httpClient,
            ILogger<DmiRadarApiClient>? logger = null)
        {

            _logger = logger ?? NullLogger<DmiRadarApiClient>.Instance;
            _httpClient = httpClient;
        }
        public async Task<DmiRadarScanFeature?> GetLatestScanForPointAsync(double latitude = 0, double longitude = 0, string collectionName = "pseudoCappi")
        {

            // Get scans from the last 15 minutes
            var now = DateTime.UtcNow;
            var scans = await GetScansAsync(now.AddMinutes(-15), now, collectionName);

            if (scans.Count == 0)
            {
                _logger.LogWarning("No recent radar scans available");
                return null;
            }
            // Find the most recent scan whose bbox contains our point
            var matchingScan = scans
                //.Where(s => ContainsPoint(s.Geometry?.Bbox, latitude, longitude))
                //.OrderByDescending(s => s.Properties.Datetime)
                .FirstOrDefault();

            if (matchingScan == null)
            {
                _logger.LogInformation(
                    "No scan covers point ({Lat}, {Lon})", latitude, longitude);
            }

            return matchingScan;
        }
        /// <summary>
        /// Check if a lat/lon falls within a bounding box.
        /// DMI returns bbox as [minLon, minLat, maxLon, maxLat]
        /// </summary>
        private static bool ContainsPoint(double[]? bbox, double lat, double lon)
        {
            if (bbox == null || bbox.Length < 4)
                return false;

            double minLon = bbox[0];
            double minLat = bbox[1];
            double maxLon = bbox[2];
            double maxLat = bbox[3];

            return lat >= minLat && lat <= maxLat &&
                   lon >= minLon && lon <= maxLon;
        }
        public async Task<List<DmiRadarScanFeature>> GetScansAsync(
            DateTime startTime,
            DateTime endTime,
            string collectionName = "pseudoCappi",
            CancellationToken cancellationToken = default)
        {
            try
            {
                //var now = DateTime.UtcNow;
                //var tenMinutesAgo = now.AddMinutes(-10);
                //var startTimeStr = tenMinutesAgo.ToString("o").Replace("+", "%2B");
                //var endTimeStr = now.ToString("o").Replace("+", "%2B");
                //var datetimeRange = $"{startTimeStr}/{endTimeStr}";

                var _startTime = new DateTime(2026, 4, 22, 4, 0, 0, DateTimeKind.Utc);
                var _endTime = new DateTime(2026, 4, 22, 4, 10, 0, DateTimeKind.Utc);
                var _datetimeRange = $"{_startTime:o}/{_endTime:o}".Replace("+", "%2B");

                //var datetimeRange = $"{startTime:yyyy-MM-ddTHH:mm:ssZ}/{endTime:yyyy-MM-ddTHH:mm:ssZ}";
                var requestPath = $"collections/{collectionName}/items?datetime={_datetimeRange}&limit=100";
                _logger.LogInformation("Final URL: {FullUrl}", _httpClient.BaseAddress + requestPath);
                var result = await _httpClient.GetFromJsonAsync<DmiRadarScansResponse>(requestPath, cancellationToken);

                if (result == null)
                {
                    _logger.LogWarning("DMI API returned no results");
                    return new List<DmiRadarScanFeature>(); // Return empty list, not null
                }
                _logger.LogInformation("Retrieved {Count} radar scans", result.Features.Count);
                return result.Features;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when fetching scans from DMI");
                return new List<DmiRadarScanFeature>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse DMI API response");
                return new List<DmiRadarScanFeature>();
            }
        }
    }
}
