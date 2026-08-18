using System.Threading.Channels;

namespace WeatherEvents.Queues
{
    public class InMemoryDeadLetterQueue<T> : IDeadLetterQueue<T>
    {
        private readonly Channel<T> _channel;
        private int _count;

        public InMemoryDeadLetterQueue()
        {
            _channel = Channel.CreateBounded<T>(10000);
        }

        public int Count => _count;

        public ValueTask EnqueueAsync(T workItem)
        {
            Interlocked.Increment(ref _count);
            return _channel.Writer.WriteAsync(workItem);
        }

        public async ValueTask<T> DequeueAsync(
            CancellationToken cancellationToken)
        {
            var item = await _channel.Reader.ReadAsync(cancellationToken);

            Interlocked.Decrement(ref _count);

            return item;
        }
    }
}