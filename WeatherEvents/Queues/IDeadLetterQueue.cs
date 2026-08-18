namespace WeatherEvents.Queues
{
    public interface IDeadLetterQueue<T>
    {
        int Count { get; }

        ValueTask EnqueueAsync(T workItem);

        ValueTask<T> DequeueAsync(
            CancellationToken cancellationToken);
    }
}