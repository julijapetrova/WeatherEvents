namespace WeatherEvents.Queues
{
    public interface IDeadLetterQueue
    {
        int Count { get; }

        ValueTask EnqueueAsync(WeatherEventWorkItem weatherEventWorkItem);

        ValueTask<WeatherEventWorkItem> DequeueAsync(
            CancellationToken cancellationToken);
    }
}