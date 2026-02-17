namespace BuildingBlocks.Messaging.Abstractions;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent _event, CancellationToken cancellationToken = default)
        where TEvent : class;
    Task SubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
        where TEvent : class
        where THandler : class;
}