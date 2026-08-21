using WeatherEvents.Queues;
using WeatherEvents.Services;

namespace WeatherEvents.Workers;

public sealed class DmiRadarProcessor : BackgroundService
{
    private readonly IDeadLetterQueue<RadarScanWorkItem> _deadLetterQueue;
    private readonly IDmiRadarEventQueue _queue;
    private readonly ILogger<DmiRadarProcessor> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

   

    public DmiRadarProcessor(
        IServiceScopeFactory scopeFactory,
        IDmiRadarEventQueue queue,
        IDeadLetterQueue<RadarScanWorkItem> deadLetterQueue,
        ILogger<DmiRadarProcessor> logger
        )
    {
        _deadLetterQueue = deadLetterQueue;
        _scopeFactory = scopeFactory;
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("DMI Radar Processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
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
                    "Unexpected error in DMI Radar Processor.");
            }
        }

        _logger.LogInformation("DMI Radar Processor stopped.");
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
                "Retrying radar scan {ScanId}. Retry {RetryCount} of 2.",
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

}