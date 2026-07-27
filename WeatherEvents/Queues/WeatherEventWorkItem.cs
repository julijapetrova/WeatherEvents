using WeatherEvents.Models;

namespace WeatherEvents.Queues
{
    public class WeatherEventWorkItem
    {
        public WeatherEvent WeatherEvent { get; init; } = default!;

        public int RetryCount { get; set; }
    }
}