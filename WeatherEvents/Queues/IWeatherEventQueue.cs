using WeatherEvents.Models;

namespace WeatherEvents.Queues
{
    public interface IWeatherEventQueue
    {
        ValueTask EnqueueAsync(WeatherEvent weatherEvent);
        ValueTask<WeatherEvent> DequeueAsync(CancellationToken stoppingToken);

    }
}
