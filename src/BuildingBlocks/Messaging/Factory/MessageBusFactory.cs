using Messaging.Interfaces;
using Messaging.MessagesBuses;

using SharedKernel.Options;

namespace Messaging.Factories
{
    public static class MessageBusFactory
    {
        public static IMessageBus Create(string providerName, MessagingOptions providerOptions, Dictionary<string, string> connections)
        {
            string connection = connections.Where(x => x.Key.Equals(providerName, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(x.Value))
                .Select(x => x.Value)
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"{providerName} connection is missing.");

            return providerName switch
            {
                "RabbitMQ" => new RabbitMQMessageBus(providerOptions.Routes, connection),
                "Kafka" => new KafkaMessageBus(providerOptions.Routes, connection),
                _ => throw new ArgumentException("Unknown provider")
            };
        }
    }
}
