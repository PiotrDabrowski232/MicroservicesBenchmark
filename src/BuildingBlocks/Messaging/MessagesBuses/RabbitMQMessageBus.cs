using Messaging.Interfaces;

using RabbitMQ.Client;

using SharedKernel.Options;

namespace Messaging.MessagesBuses
{
    public class RabbitMQMessageBus : IMessageBus
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly RabbitMqOptions _rabbitOptions;
        private readonly List<MessageRouteOptions> _messageRouteOptions;

        public RabbitMQMessageBus(RabbitMqOptions rabbitMqOptions, List<MessageRouteOptions> messageRouteOptions)
        {
            _rabbitOptions = rabbitMqOptions;
            _messageRouteOptions = messageRouteOptions;
            _connectionFactory = new ConnectionFactory { };
        }

        public Task PublishAsync<T>(T message, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task StartConsumingAsync<T>(Func<T, Task> handler)
        {
            throw new NotImplementedException();
        }
    }
}
