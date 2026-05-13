using System.Text.Json;

using Confluent.Kafka;

using Messaging.Interfaces;

using SharedKernel.Options;

namespace Messaging.MessagesBuses
{
    public class KafkaMessageBus : IMessageBus, IDisposable
    {
        private const string DefaultAcks = "All";
        private const bool DefaultEnableIdempotence = true;
        private const int DefaultLingerMs = 5;
        private const bool DefaultEnableAutoCommit = false;
        private const int DefaultCommitBatchSize = 20;
        private const string DefaultAutoOffsetReset = "Earliest";

        private readonly string _brokerConnectionString;
        private readonly List<MessageRouteOptions> _messageRouteOptions;
        private readonly IProducer<string, string> _producer;

        public KafkaMessageBus(
            List<MessageRouteOptions> messageRouteOptions,
            string brokerConnectionString)
        {
            _brokerConnectionString = brokerConnectionString;
            _messageRouteOptions = messageRouteOptions;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _brokerConnectionString,
                Acks = ParseAcks(DefaultAcks),
                EnableIdempotence = DefaultEnableIdempotence,
                LingerMs = DefaultLingerMs,
            };

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        public async Task PublishAsync<T>(T message, CancellationToken ct)
        {
            await Task.Yield();

            var route = GetMessageRoute<T>();

            if (!route.Publisher)
                throw new InvalidOperationException($"Message type {typeof(T).Name} is not allowed to be published");

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

            await _producer.ProduceAsync(route.Topic, kafkaMessage, ct);
        }

        public async Task StartConsumingAsync<T>(Func<T, Task> handler, CancellationToken ct)
        {
            var route = GetMessageRoute<T>();

            if (route.Publisher)
                throw new InvalidOperationException($"Message type {typeof(T).Name} is not a consumer.");

            var config = new ConsumerConfig
            {
                BootstrapServers = _brokerConnectionString,
                EnableAutoCommit = DefaultEnableAutoCommit,
                AutoOffsetReset = ParseAutoOffsetReset(DefaultAutoOffsetReset),
                GroupId = route.ConsumerGroup
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            var commitBatchSize = Math.Max(1, DefaultCommitBatchSize);
            var pendingCommitCount = 0;
            ConsumeResult<string, string>? lastProcessedResult = null;

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
                            TrackProcessedResult(consumeResult);
                            CommitIfNeeded();
                            continue;
                        }

                        await handler(message);

                        TrackProcessedResult(consumeResult);
                        CommitIfNeeded();
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
                CommitIfNeeded(force: true);
                consumer.Close();
            }

            await Task.CompletedTask;

            void TrackProcessedResult(ConsumeResult<string, string> consumeResult)
            {
                if (DefaultEnableAutoCommit)
                {
                    return;
                }

                lastProcessedResult = consumeResult;
                pendingCommitCount++;
            }

            void CommitIfNeeded(bool force = false)
            {
                if (DefaultEnableAutoCommit || lastProcessedResult is null)
                {
                    return;
                }

                if (!force && pendingCommitCount < commitBatchSize)
                {
                    return;
                }

                consumer.Commit(lastProcessedResult);
                lastProcessedResult = null;
                pendingCommitCount = 0;
            }
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

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(5));
            _producer.Dispose();
        }

        private static Acks ParseAcks(string? value)
        {
            return value?.Trim().ToLowerInvariant() switch
            {
                "none" or "0" => Acks.None,
                "leader" or "1" => Acks.Leader,
                _ => Acks.All
            };
        }

        private static AutoOffsetReset ParseAutoOffsetReset(string? value)
        {
            return value?.Trim().ToLowerInvariant() switch
            {
                "latest" => AutoOffsetReset.Latest,
                "error" => AutoOffsetReset.Error,
                _ => AutoOffsetReset.Earliest
            };
        }
    }
}
