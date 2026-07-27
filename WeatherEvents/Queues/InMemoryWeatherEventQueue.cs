using System.Threading.Channels;
using WeatherEvents.Controllers;
using WeatherEvents.Models;

namespace WeatherEvents.Queues
{
    public class InMemoryWeatherEventQueue : IWeatherEventQueue
    {
        private readonly Channel<WeatherEvent> _channel;
        private readonly ILogger<WeatherReadingsController> _logger;
        private int _count;

        public InMemoryWeatherEventQueue(ILogger<WeatherReadingsController> logger)
        {
            // Limit queued events to prevent unbounded memory growth if 
            // readings arrive faster than the worker can persist them.
            // This value should be tuned based on expected traffic and processing speed.
            // expected burst size x safety margin
            _channel = Channel.CreateBounded<WeatherEvent>(10000);
            _logger = logger;
        }

        public ValueTask EnqueueAsync(WeatherEvent weatherEvent)
        {
            Interlocked.Increment(ref _count);
            return _channel.Writer.WriteAsync(weatherEvent);
        }

        public async ValueTask<WeatherEvent> DequeueAsync(
    CancellationToken stoppingToken)
        {
            var item = await _channel.Reader.ReadAsync(stoppingToken);

            Interlocked.Decrement(ref _count);
            return item;
        }
        public int Count => _count;

    }
}
