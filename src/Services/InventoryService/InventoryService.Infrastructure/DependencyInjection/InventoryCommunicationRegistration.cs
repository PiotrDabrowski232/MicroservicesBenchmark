using InventoryService.Application.Async.Commands;
using InventoryService.Application.Async.Consumers;
using InventoryService.Application.Commands;

using Messaging.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SharedKernel.Factory;
using SharedKernel.Options;

namespace InventoryService.Infrastructure.DependencyInjection
{
    public static class InventoryCommunicationRegistration
    {
        public static IServiceCollection AddInventoryCommunication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var options = configuration
                .GetSection("Communication")
                .Get<CommunicationOptions>()
                ?? throw new InvalidOperationException("Communication configuration is missing.");


            services.Configure<CommunicationOptions>(configuration.GetSection("Communication"));

            CommunicationFactory.RegisterCommunicationFactories(
            services,
            options,
            RegisterSync,
            (services, options) => RegisterAsync(services, options, configuration));

            return services;
        }
        private static void RegisterSync(IServiceCollection services, CommunicationOptions options)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ReserveProductCommand).Assembly));
        }

        private static void RegisterAsync(
            IServiceCollection services,
            CommunicationOptions options,
            IConfiguration configuration)
        {
            var connections = configuration
                .GetSection("ConnectionStrings")
                .Get<Dictionary<string, string>>()
                ?? throw new InvalidOperationException("Connections configuration is missing.");

            services.AddMessaging(options, connections);

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ReserveProductAsyncCommand).Assembly));

            services.AddHostedService<ReserveProductConsumer>();
        }

    }
}
