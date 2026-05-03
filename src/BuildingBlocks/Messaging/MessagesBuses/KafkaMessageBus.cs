using System.Text.Json;

using Confluent.Kafka;

using Messaging.Interfaces;

using SharedKernel.Options;

namespace Messaging.MessagesBuses
{
    public class KafkaMessageBus : IMessageBus
    {
        private readonly string _brokerConnectionString;
        private readonly List<MessageRouteOptions> _messageRouteOptions;
        public KafkaMessageBus(List<MessageRouteOptions> messageRouteOptions, string brokerConnectionString)
        {
            _brokerConnectionString = brokerConnectionString;
            _messageRouteOptions = messageRouteOptions;
        }

        public async Task PublishAsync<T>(T message, CancellationToken ct)
        {
            await Task.Yield();

            var route = GetMessageRoute<T>();

            if (!route.Publisher)
                throw new InvalidOperationException($"Message type {typeof(T).Name} is not allowed to be published");

            var config = new ProducerConfig
            {
                BootstrapServers = _brokerConnectionString,
                Acks = Acks.All,
                EnableIdempotence = true,
            };

            using var producer = new ProducerBuilder<string, string>(config).Build();

            var jsonMessage = JsonSerializer.Serialize(message);

            var kafkaMessage = new Message<string, string>
            {
                Key = route.RoutingKey ?? typeof(T).Name,

                Value = jsonMessage,

                Headers = new Headers
                {
                    { "message-type", System.Text.Encoding.UTF8.GetBytes(typeof(T).FullName!) },
                    { "content-type", System.Text.Encoding.UTF8.GetBytes("application/json") }
                }
            };

            await producer.ProduceAsync(route.Topic, kafkaMessage, ct);
        }

        public async Task StartConsumingAsync<T>(Func<T, Task> handler, CancellationToken ct)
        {
            var route = GetMessageRoute<T>();

            if (route.Publisher)
                throw new InvalidOperationException($"Message type {typeof(T).Name} is not a consumer.");

            var config = new ConsumerConfig
            {
                BootstrapServers = _brokerConnectionString,
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                GroupId = route.ConsumerGroup
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe(route.Topic);

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(ct);

                        var message = JsonSerializer.Deserialize<T>(consumeResult.Message.Value);

                        if (message is null)
                        {
                            consumer.Commit(consumeResult);
                            continue;
                        }

                        await handler(message);

                        consumer.Commit(consumeResult);
                    }
                    catch (ConsumeException ex)
                    {
                        Console.WriteLine($"Kafka consume error: {ex.Error.Reason}");
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Kafka message deserialization error: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Kafka handler error: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                consumer.Close();
            }

            await Task.CompletedTask;
        }

        public MessageRouteOptions GetMessageRoute<T>()
        {
            var route = _messageRouteOptions
                .FirstOrDefault(m => m.MessageType == typeof(T).Name);

            if (route == null)
                throw new InvalidOperationException($"No route found for message type {typeof(T).Name}");

            if (string.IsNullOrWhiteSpace(route.Topic))
                throw new InvalidOperationException($"Kafka topic is missing for message type {typeof(T).Name}");

            return route;
        }
    }
}
