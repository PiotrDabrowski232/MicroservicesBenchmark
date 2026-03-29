namespace Messaging.Interfaces
{
    public interface IMessageConsumer
    {
        Task StartConsumingAsync<T>(Func<T, Task> handler);
    }
}
