using Messaging.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OrderService.Application.Async;
using OrderService.Application.Interfaces;
using OrderService.Application.Orders.Commands;
using OrderService.Infrastructure.HttpClients;

using SharedKernel.Factory;
using SharedKernel.Options;

using static OrderService.Infrastructure.Protos.Inventory;
using static OrderService.Infrastructure.Protos.Payment;

namespace OrderService.Infrastructure.DependencyInjection
{
    public static class OrderCommunicationRegistration
    {
        public static IServiceCollection AddOrderCommunication(
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
            SyncCommunicationFactory.RegisterSyncCommunication(
                services,
                options,
                RegisterGrpc,
                RegisterRest);

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderSyncCommand).Assembly));

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

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderAsyncCommand).Assembly));

        }

        private static void RegisterGrpc(IServiceCollection services, CommunicationOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Grpc.InventoryService))
                throw new InvalidOperationException("Communication:Grpc:InventoryService is missing.");

            if (string.IsNullOrWhiteSpace(options.Grpc.PaymentService))
                throw new InvalidOperationException("Communication:Grpc:PaymentService is missing.");

            services.AddGrpcClient<InventoryClient>(o =>
            {
                o.Address = new Uri(options.Grpc.InventoryService);
            });

            services.AddGrpcClient<PaymentClient>(o =>
            {
                o.Address = new Uri(options.Grpc.PaymentService);
            });

            services.AddScoped<IInventoryClient, GrpcInventoryClient>();
            services.AddScoped<IPaymentClient, GrpcPaymentClient>();
        }

        private static void RegisterRest(IServiceCollection services, CommunicationOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Rest.InventoryService))
                throw new InvalidOperationException("Communication:Rest:InventoryService is missing.");

            if (string.IsNullOrWhiteSpace(options.Rest.PaymentService))
                throw new InvalidOperationException("Communication:Rest:PaymentService is missing.");

            services.AddHttpClient<IInventoryClient, HttpInventoryClient>(client =>
            {
                client.BaseAddress = new Uri(options.Rest.InventoryService);
            });

            services.AddHttpClient<IPaymentClient, HttpPaymentClient>(client =>
            {
                client.BaseAddress = new Uri(options.Rest.PaymentService);
            });
        }
    }
}
