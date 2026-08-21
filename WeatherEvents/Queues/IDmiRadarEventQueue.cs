namespace WeatherEvents.Queues
{
    public interface IDmiRadarEventQueue
    {
        int Count { get; }
        ValueTask EnqueueAsync(
                RadarScanWorkItem workItem,
                CancellationToken cancellationToken = default);

        ValueTask<RadarScanWorkItem> DequeueAsync(
            CancellationToken cancellationToken);
    }
}
