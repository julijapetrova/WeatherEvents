using WeatherEvents.DTOs.DmiRadar;

namespace WeatherEvents.Services
{
    public interface IDmiRadarApiClient
    {
        /// <summary>
        /// Get the most recent radar scan that covers a given point.
        /// </summary>
        Task<DmiRadarScanFeature?> GetLatestScanForPointAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default,
            string collectionName = "pseudoCappi"

);

        /// <summary>
        /// Get all scans in a time range (for batch processing / pipeline)
        /// </summary>
        Task<List<DmiRadarScanFeature>> GetScansAsync(
            DateTime startTime,
            DateTime endTime,
            CancellationToken cancellationToken = default,
            string collectionName = "pseudoCappi"

            );
    }
}
