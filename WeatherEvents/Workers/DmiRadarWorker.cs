using WeatherEvents.DTOs.DmiRadar;
using WeatherEvents.Models;
using WeatherEvents.Queues;
using WeatherEvents.Repositories;
using WeatherEvents.Services;

namespace WeatherEvents.Workers;

public sealed class DmiRadarWorker : BackgroundService
{
    private readonly IDeadLetterQueue<RadarScanWorkItem> _deadLetterQueue;
    private readonly IDmiRadarEventQueue _queue;
    private readonly ILogger<DmiRadarWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDmiRadarApiClient _client;

    private static readonly TimeSpan PollInterval =
        TimeSpan.FromMinutes(10);

    public DmiRadarWorker(
        IServiceScopeFactory scopeFactory,
        IDmiRadarEventQueue queue,
        IDeadLetterQueue<RadarScanWorkItem> deadLetterQueue,
        ILogger<DmiRadarWorker> logger,
        IDmiRadarApiClient client)
    {
        _deadLetterQueue = deadLetterQueue;
        _scopeFactory = scopeFactory;
        _queue = queue;
        _logger = logger;
        _client = client;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("DMI Radar Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await FetchAndEnqueueScansAsync(stoppingToken);

                await ProcessQueuedScansAsync(stoppingToken);
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
                    "Unexpected error in DMI Radar Worker.");
            }

            try
            {
                await Task.Delay(
                    PollInterval,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("DMI Radar Worker stopped.");
    }

    private async Task FetchAndEnqueueScansAsync(
        CancellationToken cancellationToken)
    {
        var endTime = DateTime.UtcNow;
        var startTime = endTime.AddMinutes(-10);

        _logger.LogInformation(
            "Fetching DMI radar scans from {StartTime} to {EndTime}.",
            startTime,
            endTime);

        var scans = await _client.GetScansAsync(
            startTime,
            endTime,
            cancellationToken);

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

    private async Task ProcessQueuedScansAsync(
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            RadarScanWorkItem workItem;

            try
            {
                workItem = await _queue.DequeueAsync(
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var repository =
                    scope.ServiceProvider
                        .GetRequiredService<IRadarScanRepository>();

                var existingScan =
                    await repository.GetByScanIdAsync(
                        workItem.RadarScan.ScanId);

                if (existingScan == null)
                {
                    var createdRadarScan =
                        await repository.AddScanAsync(
                            workItem.RadarScan);

                    _logger.LogInformation(
                        "Radar scan saved: StationId={StationId}, ScanId={ScanId}, Id={Id}",
                        createdRadarScan.StationId,
                        createdRadarScan.ScanId,
                        createdRadarScan.Id);
                }
                else
                {
                    _logger.LogDebug(
                        "Radar scan {ScanId} already exists. Skipping.",
                        workItem.RadarScan.ScanId);
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                await HandleProcessingFailureAsync(
                    workItem,
                    ex,
                    cancellationToken);
            }
        }
    }

    private async Task HandleProcessingFailureAsync(
        RadarScanWorkItem workItem,
        Exception exception,
        CancellationToken cancellationToken)
    {
        workItem.RetryCount++;

        if (workItem.RetryCount < 3)
        {
            _logger.LogWarning(
                exception,
                "Retry {RetryCount} for radar scan {ScanId}.",
                workItem.RetryCount,
                workItem.RadarScan.ScanId);

            await _queue.EnqueueAsync(
                workItem,
                cancellationToken);

            return;
        }

        await _deadLetterQueue.EnqueueAsync(
            workItem);

        _logger.LogError(
            exception,
            "Moved radar scan {ScanId} to dead-letter queue after {RetryCount} attempts. Main queue: {MainQueueCount}, Dead-letter queue: {DeadLetterQueueCount}.",
            workItem.RadarScan.ScanId,
            workItem.RetryCount,
            _queue.Count,
            _deadLetterQueue.Count);
    }

    private static RadarScan ConvertToEntity(
        DmiRadarScanFeature scan)
    {
        // We will implement this after looking at
        // your actual DMI DTO and RadarScan model.
        throw new NotImplementedException();
    }
}