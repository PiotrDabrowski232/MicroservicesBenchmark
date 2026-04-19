namespace Messaging.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, CancellationToken ct);
    }

}
