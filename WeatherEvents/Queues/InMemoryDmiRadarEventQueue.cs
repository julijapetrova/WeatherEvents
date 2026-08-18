using System.Threading.Channels;
using WeatherEvents.DTOs.DmiRadar;
using WeatherEvents.Models;

namespace WeatherEvents.Queues
{
    public class InMemoryDmiRadarEventQueue : IDmiRadarEventQueue
    {
        private readonly Channel<RadarScanWorkItem> _channel;
        private int _count;
        public int Count => _count;
        public InMemoryDmiRadarEventQueue()
        {
          
            _channel = Channel.CreateBounded<RadarScanWorkItem>(10000);
        }
        public async ValueTask<RadarScanWorkItem> DequeueAsync(CancellationToken cancellationToken)
        {
            var radarScan = await _channel.Reader.ReadAsync(cancellationToken);

            Interlocked.Decrement(ref _count);

            return radarScan;
        }

        public ValueTask EnqueueAsync(RadarScanWorkItem radarScanWorkItem)
        {
            Interlocked.Increment(ref _count);
            return _channel.Writer.WriteAsync(radarScanWorkItem);
        }
    }
}
