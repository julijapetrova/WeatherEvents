using WeatherEvents.Queues;
using WeatherEvents.Repositories;

namespace WeatherEvents.Workers
{
    public class WeatherEventWorker : BackgroundService
    {
        private readonly IWeatherEventQueue _queue;
        private readonly IDeadLetterQueue<WeatherEventWorkItem> _deadLetterQueue;
        private readonly ILogger<WeatherEventWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public WeatherEventWorker(
            IServiceScopeFactory scopeFactory,
            IWeatherEventQueue queue,
            IDeadLetterQueue<WeatherEventWorkItem> deadLetterQueue,
            ILogger<WeatherEventWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _queue = queue;
            _deadLetterQueue = deadLetterQueue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                WeatherEventWorkItem workItem = await _queue.DequeueAsync(stoppingToken);

                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var repository = scope.ServiceProvider
                        .GetRequiredService<IWeatherRepository>();

                    var createdEvent =
                        await repository.AddReadingAsync(workItem.WeatherEvent);

                    _logger.LogInformation(
                        "Weather reading saved: StationId={StationId}, SequenceNumber={SequenceNumber}, Id={Id}",
                        createdEvent.StationId,
                        createdEvent.SequenceNumber,
                        createdEvent.Id);
                }
                catch (Exception ex)
                {
                    workItem.RetryCount++;

                    if (workItem.RetryCount < 3)
                    {
                        _logger.LogWarning(
                            "Retry {RetryCount} for weather event {SequenceNumber}",
                            workItem.RetryCount,
                            workItem.WeatherEvent.SequenceNumber);

                        await _queue.EnqueueAsync(workItem);
                    }
                    else
                    {
                        await _deadLetterQueue.EnqueueAsync(workItem);

                        _logger.LogError(
                          ex,
                          "Moved weather event {SequenceNumber} to dead-letter queue after {RetryCount} attempts. " +
                          "Main queue: {MainQueueCount}, Dead-letter queue: {DeadLetterCount}",
                          workItem.WeatherEvent.SequenceNumber,
                          workItem.RetryCount,
                          _queue.Count,
                          _deadLetterQueue.Count);
                    }
                }
            }
        }
    }
}