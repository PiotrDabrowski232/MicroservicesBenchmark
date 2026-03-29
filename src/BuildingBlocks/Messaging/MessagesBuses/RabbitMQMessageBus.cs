using Messaging.Interfaces;

namespace Messaging.MessagesBuses
{
    public class RabbitMQMessageBus : IMessageBus
    {
        public RabbitMQMessageBus(string v)
        {
        }

        public Task StartConsumingAsync<T>(Func<T, Task> handler)
        {
            throw new NotImplementedException();
        }
    }
}
