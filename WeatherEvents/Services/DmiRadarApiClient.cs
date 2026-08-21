using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using WeatherEvents.DTOs.DmiRadar;
using System.Globalization;
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
        public async Task<DmiRadarScanFeature?> GetLatestScanForPointAsync(
            double latitude = 0,
            double longitude = 0,
            CancellationToken cancellationToken = default,
            string collectionName = "pseudoCappi")
        {

            // Get scans from the last 15 minutes
            var now = DateTime.UtcNow;
            var scans = await GetScansAsync(
                now.AddMinutes(-15),
    now,
    cancellationToken: cancellationToken,
    collectionName: collectionName);
            if (scans.Count == 0)
            {
                _logger.LogInformation("No recent radar scans available.");
                return null;
            }
            // Find the most recent scan whose bbox contains our point
            var matchingScan = scans
                .Where(s => ContainsPoint(s.Geometry?.Bbox, latitude, longitude))
                .OrderByDescending(s => s.Properties.Datetime)
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
    CancellationToken cancellationToken = default,
    string collectionName = "pseudoCappi")
        {
            try
            {
                var datetimeRange =
                    $"{FormatRfc3339(startTime)}/{FormatRfc3339(endTime)}";
                var requestPath = $"collections/{collectionName}/items?datetime={datetimeRange}&limit=100";
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
        private static string FormatRfc3339(DateTime value)
        {
            return value.ToString(
                "yyyy-MM-dd'T'HH':'mm':'ss'Z'",
                CultureInfo.InvariantCulture);
        }
    }

}
