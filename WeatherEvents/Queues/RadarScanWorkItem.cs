using WeatherEvents.Models;

namespace WeatherEvents.Queues
{
    public class RadarScanWorkItem
    {
        public RadarScan RadarScan { get; init; } = default!;

        public int RetryCount { get; set; }
    }
}
