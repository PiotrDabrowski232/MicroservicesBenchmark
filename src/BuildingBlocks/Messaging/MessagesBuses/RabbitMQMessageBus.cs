using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

using Messaging.Interfaces;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using SharedKernel.Options;

namespace Messaging.MessagesBuses
{
    public class RabbitMQMessageBus : IMessageBus, IAsyncDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly List<MessageRouteOptions> _messageRouteOptions;
        private readonly string _brokerConnectionString;

        // Wspoldzielone, dlugozyjace polaczenie do publikacji (odpowiednik jednego producenta Kafki).
        // Kanaly nie sa thread-safe, dlatego uzywana jest ich pula - kazda publikacja wypozycza
        // wlasny kanal ze wspoldzielonego polaczenia, zamiast otwierac nowe polaczenie na kazda wiadomosc.
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private readonly ConcurrentQueue<IChannel> _channelPool = new();
        private IConnection? _publishConnection;
        private bool _exchangesDeclared;

        public RabbitMQMessageBus(List<MessageRouteOptions> messageRouteOptions, string brokerConnectionString)
        {
            _messageRouteOptions = messageRouteOptions;
            _brokerConnectionString = brokerConnectionString;
            _connectionFactory = new ConnectionFactory { Uri = new Uri(_brokerConnectionString) };
        }

        private async Task<IConnection> GetPublishConnectionAsync(CancellationToken ct)
        {
            if (_publishConnection is { IsOpen: true })
                return _publishConnection;

            await _connectionLock.WaitAsync(ct);
            try
            {
                if (_publishConnection is null || !_publishConnection.IsOpen)
                {
                    _publishConnection = await _connectionFactory.CreateConnectionAsync(ct);
                    _exchangesDeclared = false;
                }

                if (!_exchangesDeclared)
                {
                    await using var setupChannel = await _publishConnection.CreateChannelAsync(cancellationToken: ct);
                    foreach (var exchange in _messageRouteOptions
                        .Where(r => r.Publisher && !string.IsNullOrWhiteSpace(r.Exchange))
                        .Select(r => r.Exchange)
                        .Distinct())
                    {
                        await setupChannel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: ct);
                    }
                    _exchangesDeclared = true;
                }

                return _publishConnection;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public async Task PublishAsync<T>(T message, CancellationToken ct)
        {
            var route = GetMessageRoute<T>();

            if (!route.Publisher)
                throw new InvalidOperationException($"Message type {typeof(T).Name} is not a publisher.");

            var connection = await GetPublishConnectionAsync(ct);

            if (!_channelPool.TryDequeue(out var channel) || !channel.IsOpen)
                channel = await connection.CreateChannelAsync(cancellationToken: ct);

            try
            {
                var serializedMessage = JsonSerializer.Serialize(message);
                ReadOnlyMemory<byte> body = Encoding.UTF8.GetBytes(serializedMessage);

                await channel.BasicPublishAsync(
                    exchange: route.Exchange,
                    routingKey: route.RoutingKey ?? string.Empty,
                    mandatory: false,
                    basicProperties: new BasicProperties
                    {
                        Persistent = true,
                        ContentType = "application/json",
                        Type = typeof(T).FullName
                    },
                    body: body,
                    cancellationToken: ct);
            }
            finally
            {
                if (channel.IsOpen)
                    _channelPool.Enqueue(channel);
                else
                    await channel.DisposeAsync();
            }
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

        public async ValueTask DisposeAsync()
        {
            while (_channelPool.TryDequeue(out var channel))
                await channel.DisposeAsync();

            if (_publishConnection is not null)
                await _publishConnection.DisposeAsync();

            _connectionLock.Dispose();
        }
    }
}
