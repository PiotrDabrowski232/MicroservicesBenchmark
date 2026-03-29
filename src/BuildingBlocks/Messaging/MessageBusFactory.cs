using Messaging.Interfaces;
using Messaging.MessagesBuses;

using Microsoft.Extensions.Configuration;

namespace Messaging
{
    public static class MessageBusFactory
    {
        public static IMessageBus Create(string provider, IConfiguration config)
        {
            return provider switch
            {
                "RabbitMQ" => new RabbitMQMessageBus(config["Messaging:RabbitMQ:Host"] ?? "localhost"),
                "Kafka" => new KafkaMessageBus(config["Messaging:Kafka:BootstrapServers"] ?? "localhost:9092"),
                _ => throw new ArgumentException("Unknown provider")
            };
        }
    }
}
