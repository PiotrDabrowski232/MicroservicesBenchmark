namespace Messaging.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, string topicName, string correlationId = null);
    }

}
