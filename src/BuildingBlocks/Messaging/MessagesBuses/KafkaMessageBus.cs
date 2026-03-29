using System.Text;
using System.Text.Json;

using Confluent.Kafka;

using Messaging.Interfaces;

namespace Messaging.MessagesBuses
{
    public class KafkaMessageBus : IMessageBus
    {
        private readonly IProducer<string, string> _producer;
        private readonly IConsumer<string, string> _consumer;

        public KafkaMessageBus(string server)
        {
            var producerConfig = new ProducerConfig { BootstrapServers = server };
            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = server,
                GroupId = "order-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        }

        public async Task PublishAsync<T>(T message, string topicName, string correlationId = null)
        {
            var key = correlationId ?? Guid.NewGuid().ToString();
            var value = JsonSerializer.Serialize(message);
            var headers = new Headers();
            if (correlationId != null) headers.Add("correlation-id", Encoding.UTF8.GetBytes(correlationId));
            await _producer.ProduceAsync(topicName, new Message<string, string> { Key = key, Value = value, Headers = headers });
        }

        public async Task StartConsumingAsync<T>(Func<T, Task> handler)
        {
            _consumer.Subscribe("responses");
            while (true)
            {
                var result = _consumer.Consume();
                var message = JsonSerializer.Deserialize<T>(result.Message.Value);
                await handler(message);
            }
        }
    }
}
