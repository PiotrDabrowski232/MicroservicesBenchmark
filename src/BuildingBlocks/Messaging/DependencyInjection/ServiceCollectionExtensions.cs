using Messaging.Factories;
using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using SharedKernel.Options;

namespace Messaging.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessaging(
            this IServiceCollection services,
            CommunicationOptions options)
        {
            services.AddSingleton<IMessageBus>(_ =>
                MessageBusFactory.Create(options.AsyncProvider, options.Messaging));

            return services;
        }
    }
}
