namespace Messaging.Interfaces
{
    public interface IMessageConsumer
    {
        public Task StartConsumingAsync<T>(Func<T, Task> handler, CancellationToken ct);
    }
}
