using Messaging.Interfaces;
using Messaging.MessagesBuses;

using SharedKernel.Options;

namespace Messaging.Factories
{
    public static class MessageBusFactory
    {
        public static IMessageBus Create(string providerName, MessagingOptions providerOptions)
        {
            return providerName switch
            {
                "RabbitMQ" => new RabbitMQMessageBus(providerOptions.RabbitMq, providerOptions.Routes),
                "Kafka" => new KafkaMessageBus(providerOptions.Kafka, providerOptions.Routes),
                _ => throw new ArgumentException("Unknown provider")
            };
        }
    }
}
