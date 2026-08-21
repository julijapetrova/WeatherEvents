using System.Threading.Channels;

namespace WeatherEvents.Queues
{
    public class InMemoryDmiRadarEventQueue : IDmiRadarEventQueue
    {
        private readonly Channel<RadarScanWorkItem> _channel;
        private int _count;
        public int Count => _count;
        public InMemoryDmiRadarEventQueue()
        {
            _channel = Channel.CreateBounded<RadarScanWorkItem>(
            new BoundedChannelOptions(10_000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });
        }

        public async ValueTask EnqueueAsync(RadarScanWorkItem workItem,
        CancellationToken cancellationToken = default)
        {
            await _channel.Writer.WriteAsync(
            workItem,
            cancellationToken);

            Interlocked.Increment(ref _count);
        }
        public async ValueTask<RadarScanWorkItem> DequeueAsync(CancellationToken cancellationToken)
        {
            var workItem = await _channel.Reader.ReadAsync(
            cancellationToken);

            Interlocked.Decrement(ref _count);

            return workItem;
        }
    }
}
