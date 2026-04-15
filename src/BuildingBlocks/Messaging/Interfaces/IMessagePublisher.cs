namespace Messaging.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, string correlationId = null, string topicName = null);
    }

}
