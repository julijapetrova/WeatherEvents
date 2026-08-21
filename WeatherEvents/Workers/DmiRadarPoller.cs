using WeatherEvents.DTOs.DmiRadar;
using WeatherEvents.Models;
using WeatherEvents.Queues;
using WeatherEvents.Services;

namespace WeatherEvents.Workers
{
    public class DmiRadarPoller : BackgroundService
    {
        private readonly ILogger<DmiRadarPoller> _logger;
        private readonly IDmiRadarEventQueue _queue;
        private readonly IDmiRadarApiClient _client;
        private static readonly TimeSpan PollInterval =
       TimeSpan.FromMinutes(10);
        public DmiRadarPoller(
            IDmiRadarEventQueue queue,
            IDmiRadarApiClient client,
            ILogger<DmiRadarPoller> logger)
        {
            _queue = queue;
            _client = client;
            _logger = logger;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DmiRadarPoller ExecuteAsync entered.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await FetchAndEnqueueScansAsync(stoppingToken);
                    await Task.Delay(PollInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error while polling DMI radar data.");
                }
            }

        }
        private async Task FetchAndEnqueueScansAsync(
            CancellationToken cancellationToken)
        {
            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddMinutes(-30);

            _logger.LogInformation(
                "Fetching DMI radar scans from {StartTime} to {EndTime}.",
                startTime,
                endTime);

            var scans = await _client.GetScansAsync(
                startTime,
                endTime,
                cancellationToken
                );

            foreach (var scan in scans)
            {
                var radarScan = ConvertToEntity(scan);

                var workItem = new RadarScanWorkItem
                {
                    RadarScan = radarScan,
                    RetryCount = 0
                };

                await _queue.EnqueueAsync(
                    workItem,
                    cancellationToken);
            }

            _logger.LogInformation(
                "Enqueued {ScanCount} DMI radar scans. Queue count: {QueueCount}.",
                scans.Count,
                _queue.Count);
        }
        private static RadarScan ConvertToEntity(
    DmiRadarScanFeature scan)
        {
            return new RadarScan()
            {
                ScanId = scan.Id,
                ScanTime = scan.Properties.Datetime,
                StationId = scan.Properties.StationId,
                Bbox = scan.Bbox,
                DownloadUrl = scan.DownloadUrl,
                IngestedAt = DateTime.UtcNow
            };
        }

    }
}