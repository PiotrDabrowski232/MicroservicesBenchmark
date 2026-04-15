namespace SharedKernel.Options
{
    public class MessagingOptions
    {
        public RabbitMqOptions RabbitMq { get; set; } = new();
        public KafkaOptions Kafka { get; set; } = new();
        public List<MessageRouteOptions> Routes { get; set; } = new();
    }
}
