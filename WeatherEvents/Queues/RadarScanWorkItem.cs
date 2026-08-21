using WeatherEvents.Models;

namespace WeatherEvents.Queues
{
    public class RadarScanWorkItem
    {
        public required RadarScan RadarScan { get; init; };

        public int RetryCount { get; set; }
    }
}
