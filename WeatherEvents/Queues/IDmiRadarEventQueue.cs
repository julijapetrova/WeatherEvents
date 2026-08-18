using WeatherEvents.DTOs.DmiRadar;
using WeatherEvents.Models;

namespace WeatherEvents.Queues
{
    public interface IDmiRadarEventQueue
    {
        int Count { get; }

        ValueTask EnqueueAsync(RadarScanWorkItem radarScan);

        ValueTask<RadarScanWorkItem> DequeueAsync(CancellationToken cancellationToken);
    }
}
