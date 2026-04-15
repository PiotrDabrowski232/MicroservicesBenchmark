namespace SharedKernel.Options
{
    public class MessageRouteOptions
    {
        public string MessageType { get; set; } = string.Empty;
        public bool Publisher { get; set; }
        public string? Queue { get; set; }
        public string? RoutingKey { get; set; }
        public string? Topic { get; set; }
        public string? ConsumerGroup { get; set; }
    }
}
