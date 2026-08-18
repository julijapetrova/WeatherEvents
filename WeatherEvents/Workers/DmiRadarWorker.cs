using WeatherEvents.Queues;
using WeatherEvents.Repositories;
using WeatherEvents.Services;

namespace WeatherEvents.Workers
{

    public class DmiRadarWorker : BackgroundService
    {
        private readonly IDeadLetterQueue<RadarScanWorkItem> _deadLetterQueue;
        private readonly IDmiRadarEventQueue _queue;
        private readonly ILogger<DmiRadarWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDmiRadarApiClient _client;

        public DmiRadarWorker(
           IServiceScopeFactory scopeFactory,
           IDmiRadarEventQueue queue,
           IDeadLetterQueue<RadarScanWorkItem> deadLetterQueue,
           ILogger<DmiRadarWorker> logger,
           IDmiRadarApiClient client
)
        {
            _deadLetterQueue = deadLetterQueue;
            _scopeFactory = scopeFactory;
            _queue = queue;
            _logger = logger;
            _client = client;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var now = DateTime.UtcNow;
                var tenMinutesAgo = now.AddMinutes(-10);

                var scans = await _client.GetScansAsync(tenMinutesAgo, now);

                foreach (var scan in scans)
                {
                    var workItem = new RadarScanWorkItem
                    {
                        //RadarScan = ConvertToEntity(scan),  // Convert DTO → Entity
                        RetryCount = 0
                    };
                    await _queue.EnqueueAsync(workItem);
                }
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                RadarScanWorkItem workItem = await _queue.DequeueAsync(stoppingToken);

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var repository = scope.ServiceProvider
                        .GetRequiredService<IRadarScanRepository>();
                    if (await repository.GetByScanIdAsync(workItem.RadarScan.ScanId) == null)
                    {
                        var createdRadarScan =
                        await repository.AddScanAsync(workItem.RadarScan);
                        _logger.LogInformation(
                            "Radar scan saved: StationId={StationId}, ScanId={ScanId}, Id={Id}",
                            createdRadarScan.StationId,
                            createdRadarScan.ScanId,
                            createdRadarScan.Id);
                    }
                }
                catch (Exception ex)
                {
                    workItem.RetryCount++;
                    if (workItem.RetryCount < 3)
                    {
                        _logger.LogWarning(
                            "Retry {RetryCount} for radar scan {ScanId}",
                            workItem.RetryCount,
                            workItem.RadarScan.ScanId);

                        await _queue.EnqueueAsync(workItem);
                    }
                    else
                    {
                        await _deadLetterQueue.EnqueueAsync(workItem);

                        _logger.LogError(
                          ex,
                          "Moved radar scan {ScanId} to dead-letter queue after {RetryCount} attempts. Main queue: {MainQueueCount}, Dead-letter queue: {DeadLetterCount}",
                          workItem.RadarScan.ScanId,
                          workItem.RetryCount,
                          _queue.Count,
                          _deadLetterQueue.Count);
                    }
                }
            }
        }
    }
}