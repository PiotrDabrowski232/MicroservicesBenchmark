using Microsoft.Extensions.DependencyInjection;

using SharedKernel.Options;

namespace SharedKernel.Factory
{
    public static class SyncCommunicationFactory
    {
        public static void RegisterSyncCommunication(
            IServiceCollection services,
            CommunicationOptions options,
            Action<IServiceCollection, CommunicationOptions> grpcRegistration,
            Action<IServiceCollection, CommunicationOptions> restRegistration)
        {
            if (options.SyncProvider.Equals("Grpc", StringComparison.OrdinalIgnoreCase))
            {
                grpcRegistration(services, options);
                return;
            }

            if (options.SyncProvider.Equals("Rest", StringComparison.OrdinalIgnoreCase))
            {
                restRegistration(services, options);
                return;
            }

            throw new ArgumentException($"Unsupported sync provider: {options.SyncProvider}");
        }
    }
}
