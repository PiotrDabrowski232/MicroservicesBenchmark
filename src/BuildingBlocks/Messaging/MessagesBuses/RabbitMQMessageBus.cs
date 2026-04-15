using System.Data.Common;
using System.Text;
using System.Text.Json;

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

        private async Task<IConnection> ConnectQueue(CancellationToken ct)
        {
            return await _connectionFactory.CreateConnectionAsync();
        }

        private async Task EnsureExchanges(CancellationToken ct)
        {
            /*var connection = await ConnectQueue(ct);
            using var channel = await connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: ct);
            await channel.ExchangeDeclareAsync(exchange: _responseExchange, type: ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: ct);*/
        }

        public Task PublishAsync<T>(T message, string correlationId = null, string topicName = null)
        {
            /*using var channel = _connection.CreateModel();

            channel.QueueDeclare("inventory-queue", true, false, false, null);
            channel.QueueDeclare("payment-queue", true, false, false, null);
            channel.QueueDeclare("order-responses", true, false, false, null);
            channel.QueueBind("inventory-queue", _exchangeName, "inventory.reserve");
            channel.QueueBind("payment-queue", _exchangeName, "payment.process");
            channel.QueueBind("order-responses", _responseExchange, "order.response");

            var props = channel.CreateBasicProperties();
            props.Persistent = true;
            props.CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            props.ReplyTo = "order-responses";

            var routingKey = topicName ?? GetRoutingKey(message);
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            channel.BasicPublish(_exchangeName, routingKey, props, body);*/
            return Task.CompletedTask;
        }

        public Task StartConsumingAsync<T>(Func<T, Task> handler)
        {
            /*string queueName = typeof(T).Name switch
            {
                "ReserveProductMessage" => "inventory-queue",
                "ProcessPaymentMessage" => "payment-queue",
                _ => throw new InvalidOperationException("Unknown consumer type")
            };

            var channel = _connection.CreateModel();
            channel.QueueDeclare(queueName, true, false, false, null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (sender, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var payload = JsonSerializer.Deserialize<T>(json);
                    if (payload != null)
                    {
                        await handler(payload);
                    }

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch
                {
                    channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            channel.BasicConsume(queueName, autoAck: false, consumer);*/
            return Task.CompletedTask;
        }

        private string GetRoutingKey<T>(T message)
        {
            return typeof(T).Name switch
            {
                "ReserveProductMessage" => "inventory.reserve",
                "ProcessPaymentMessage" => "payment.process",
                _ => throw new ArgumentException("Unknown message type")
            };
        }
    }
}
