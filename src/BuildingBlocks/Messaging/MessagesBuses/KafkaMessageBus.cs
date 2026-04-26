using System.Text;
using System.Text.Json;

using Confluent.Kafka;

using Messaging.Interfaces;

using SharedKernel.Options;

namespace Messaging.MessagesBuses
{
    public class KafkaMessageBus : IMessageBus
    {
        private readonly IProducer<string, string> _producer;
        private readonly IConsumer<string, string> _consumer;
        private readonly string _brokerConnectionString;
        private readonly List<MessageRouteOptions> _messageRouteOptions;
        public KafkaMessageBus(string brokerConnectionString, List<MessageRouteOptions> messageRouteOptions)
        {
            _brokerConnectionString = brokerConnectionString;
            _messageRouteOptions = messageRouteOptions;
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
