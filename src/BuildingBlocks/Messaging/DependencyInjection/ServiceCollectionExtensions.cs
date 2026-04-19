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
        IConfiguration configuration)
        {
            var communicationOptions = configuration
                .GetSection("Communication")
                .Get<CommunicationOptions>()
                ?? throw new InvalidOperationException("Communication configuration is missing.");

            return services.AddMessaging(communicationOptions);
        }

        public static IServiceCollection AddMessaging(
            this IServiceCollection services,
            CommunicationOptions communicationOptions)
        {
            services.AddSingleton<IMessageBus>(_ =>
                MessageBusFactory.Create(
                    communicationOptions.AsyncProvider,
                    communicationOptions.Messaging));

            return services;
        }
    }
}
