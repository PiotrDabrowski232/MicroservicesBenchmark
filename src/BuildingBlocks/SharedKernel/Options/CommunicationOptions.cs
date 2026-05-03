using SharedKernel.Models;

namespace SharedKernel.Options
{
    public class CommunicationOptions
    {
        public string Mode { get; set; }// Sync | Async
        public string SyncProvider { get; set; }// Grpc | Rest
        public string AsyncProvider { get; set; } // RabbitMQ | Kafka
        public SyncEndpointsOptions Grpc { get; set; } = new();
        public SyncEndpointsOptions Rest { get; set; } = new(); 
        public MessagingOptions Messaging { get; set; } = new();
    }
}
