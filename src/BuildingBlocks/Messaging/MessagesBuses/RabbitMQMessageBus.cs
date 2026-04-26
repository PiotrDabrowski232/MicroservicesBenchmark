using System.Text;
using System.Text.Json;

using Messaging.Interfaces;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using SharedKernel.Options;

namespace Messaging.MessagesBuses
{
    public class RabbitMQMessageBus : IMessageBus
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly List<MessageRouteOptions> _messageRouteOptions;
        private readonly string _brokerConnectionString;

        public RabbitMQMessageBus(List<MessageRouteOptions> messageRouteOptions, string brokerConnectionString)
        {
            _messageRouteOptions = messageRouteOptions;
            _brokerConnectionString = brokerConnectionString;
            _connectionFactory = new ConnectionFactory { Uri = new Uri(_brokerConnectionString) };
        }

        public async Task PublishAsync<T>(T message, CancellationToken ct)
        {
            var route = GetMessageRoute<T>();

            if(!route.Publisher)
                throw new InvalidOperationException($"Message type {typeof(T).Name} is not a publisher.");

            var serializedMessage = System.Text.Json.JsonSerializer.Serialize(message);
            ReadOnlyMemory<byte> body = Encoding.UTF8.GetBytes(serializedMessage);

            using var connection = await _connectionFactory.CreateConnectionAsync(ct);
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(route.Exchange, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: ct);
            await channel.BasicPublishAsync(exchange: route.Exchange, routingKey: route.RoutingKey, mandatory: false,
                basicProperties: new BasicProperties {
                    Persistent = true,
                    ContentType = "application/json",
                    Type = typeof(T).FullName
                }, body: body, cancellationToken: ct);

        }

        public async Task StartConsumingAsync<T>(Func<T, Task> handler, CancellationToken ct)
        {
            var route = GetMessageRoute<T>();

            if (route.Publisher)
                throw new InvalidOperationException($"Message type {typeof(T).Name} is not a consumer.");

            using var connection = await _connectionFactory.CreateConnectionAsync(ct);
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(route.Exchange, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: ct);
            await channel.QueueDeclareAsync(queue: route.Queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
            await channel.QueueBindAsync(queue: route.Queue, exchange: route.Exchange, routingKey: route.RoutingKey, cancellationToken: ct);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);

                    var message = JsonSerializer.Deserialize<T>(messageJson);

                    if (message is null)
                    {
                        await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
                        return;
                    }

                    await handler(message);

                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch
                {
                    await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true);
                }
            };

            await channel.BasicConsumeAsync(queue: route.Queue, autoAck: false, consumer: consumer, cancellationToken: ct);
            await Task.Delay(Timeout.Infinite, ct);
        }

        public MessageRouteOptions GetMessageRoute<T>()
        {
            var route = _messageRouteOptions.FirstOrDefault(m => m.MessageType == typeof(T).Name);
            if (route == null)
                throw new InvalidOperationException($"No route found for message type {typeof(T).Name}");
            return route;
        }
    }
}
