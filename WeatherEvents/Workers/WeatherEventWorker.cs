using FluentValidation;
using WeatherEvents.Controllers;
using WeatherEvents.DTOs;
using WeatherEvents.Queues;
using WeatherEvents.Repositories;

namespace WeatherEvents.Workers
{
    public class WeatherEventWorker : BackgroundService
    {
        private readonly IWeatherEventQueue _queue;
        private readonly ILogger<WeatherEventWorker> _logger;

        private readonly IServiceScopeFactory _scopeFactory;
        public WeatherEventWorker(
            IServiceScopeFactory scopeFactory,
       IWeatherEventQueue queue,
          ILogger<WeatherEventWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var weatherEvent = await _queue.DequeueAsync(stoppingToken);

                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var repository = scope.ServiceProvider
                        .GetRequiredService<IWeatherRepository>();

                    var createdEvent = await repository.AddReadingAsync(weatherEvent);

                    _logger.LogInformation(
                       "Weather reading saved: StationId={StationId}, SequenceNumber={SequenceNumber}, Id={Id}",
                       createdEvent.StationId,
                       createdEvent.SequenceNumber,
                       createdEvent.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed processing queued weather event");
                }
            }
        }

    }
}
