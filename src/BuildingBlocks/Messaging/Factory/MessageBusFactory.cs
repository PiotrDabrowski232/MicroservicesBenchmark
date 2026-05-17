using Messaging.Initializers;
using Messaging.Interfaces;
using Messaging.MessagesBuses;

using SharedKernel.Options;

namespace Messaging.Factories
{
    public static class MessageBusFactory
    {
        public static IMessageBus Create(CommunicationOptions communicationOptions, Dictionary<string, string> connections)
        {
            var providerName = communicationOptions.AsyncProvider;
            var providerOptions = communicationOptions.Messaging;
            var normalizedProviderName = providerName?.Trim().ToLowerInvariant();

            string connection = connections.Where(x => x.Key.Equals(providerName, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(x.Value))
                .Select(x => x.Value)
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"{providerName} connection is missing.");

            return normalizedProviderName switch
            {
                "rabbitmq" => new RabbitMQMessageBus(providerOptions.Routes, connection),
                "lavinmq" => new LavinMQMessageBus(providerOptions.Routes, connection),
                "kafka" => new KafkaMessageBus(providerOptions.Routes, connection),
                _ => throw new ArgumentException($"Unknown provider '{providerName}'.")
            };
        }
        public static async Task InitializeAsync(
            string providerName,
            MessagingOptions providerOptions,
            Dictionary<string, string> connections,
            CancellationToken ct = default)
        {
            string connection = connections
                .Where(x => x.Key.Equals(providerName, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(x.Value))
                .Select(x => x.Value)
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"{providerName} connection is missing.");

            if (providerName.Equals("Kafka", StringComparison.OrdinalIgnoreCase))
            {
                var initializer = new TopicInitializers(providerOptions.Routes, connection);
                await initializer.InitializeAsync(ct);
            }
        }

    }
}
