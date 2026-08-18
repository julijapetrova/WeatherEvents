using System.Threading.Channels;

namespace WeatherEvents.Queues
{
    public class InMemoryWeatherEventQueue : IWeatherEventQueue
    {
        private readonly Channel<WeatherEventWorkItem> _channel;
        private int _count;
        public int Count => _count;

        public InMemoryWeatherEventQueue()
        {
            // Prevent unlimited memory usage if requests arrive
            // faster than they can be processed.
            _channel = Channel.CreateBounded<WeatherEventWorkItem>(10000);
        }


        public ValueTask EnqueueAsync(WeatherEventWorkItem weatherEventWorkItem)
        {
            Interlocked.Increment(ref _count);
            return _channel.Writer.WriteAsync(weatherEventWorkItem);
        }

        public async ValueTask<WeatherEventWorkItem> DequeueAsync(
            CancellationToken cancellationToken)
        {
            var item = await _channel.Reader.ReadAsync(cancellationToken);

            Interlocked.Decrement(ref _count);

            return item;
        }
    }
}