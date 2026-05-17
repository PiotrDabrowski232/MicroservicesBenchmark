using SharedKernel.Options;

namespace Messaging.MessagesBuses
{
    public sealed class LavinMQMessageBus : RabbitMQMessageBus
    {
        public LavinMQMessageBus(
            List<MessageRouteOptions> messageRouteOptions,
            string brokerConnectionString)
            : base(messageRouteOptions, brokerConnectionString)
        {
        }
    }
}
