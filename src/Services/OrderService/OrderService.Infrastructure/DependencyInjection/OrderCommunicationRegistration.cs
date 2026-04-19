using MediatR;

using Messaging.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OrderService.Application.Commands;
using OrderService.Application.DTOs;
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
                RegisterAsync);

            return services;
        }

        private static void RegisterSync(IServiceCollection services, CommunicationOptions options)
        {
            SyncCommunicationFactory.RegisterSyncCommunication(
                services,
                options,
                RegisterGrpc,
                RegisterRest);

            /*
             Reczna rejestracja handlera dla sync, bo jest tylko jeden i bym musiał w komunikacji
             synch tworzyć messaging bo przy skanowaniu całego assembly zeskanowałby
             by również moj asynchroniczny handler z imessagebus a bez sensu jest go tworzyć w wersji sync
            */

            services.AddTransient<IRequestHandler<CreateOrderSyncCommand, Guid>, CreateOrderSyncCommandHandler>();
        }

        private static void RegisterAsync(IServiceCollection services, CommunicationOptions options)
        {
            services.AddMessaging(options);

            services.AddTransient<IRequestHandler<CreateOrderAsyncCommand, OrderDto>, CreateOrderAsyncCommandHandler>();
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
