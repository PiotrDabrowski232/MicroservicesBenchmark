using Messaging.Factories;
using Messaging.HostedServices;
using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using SharedKernel.Options;

namespace Messaging.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessaging(
            this IServiceCollection services,
            CommunicationOptions communicationOptions,
            Dictionary<string, string> connections)
        {
            services.AddSingleton<IMessageBus>(_ =>
                MessageBusFactory.Create(
                    communicationOptions.AsyncProvider,
                    communicationOptions.Messaging,
                    connections));

            if (communicationOptions.AsyncProvider.Equals("Kafka", StringComparison.OrdinalIgnoreCase))
            {
                services.AddSingleton(communicationOptions);
                services.AddSingleton(connections);
                services.AddHostedService<KafkaTopicInitializerHostedService>();
            }

            return services;
        }
    }
}
