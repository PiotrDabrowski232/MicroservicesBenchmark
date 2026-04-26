using Messaging.Factories;
using Messaging.Interfaces;

using Microsoft.Extensions.Configuration;
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

            return services;
        }
    }
}
