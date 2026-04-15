using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SharedKernel.Options;

namespace SharedKernel.Factory
{
    public static class CommunicationFactory
    {
        public static void RegisterCommunicationFactories(
            IServiceCollection services,
            CommunicationOptions options,
            Action<IServiceCollection, CommunicationOptions> syncRegistration,
            Action<IServiceCollection, CommunicationOptions> asyncRegistration)
        {
            if (options.Mode.Equals("Sync", StringComparison.OrdinalIgnoreCase))
            {
                syncRegistration(services, options);
                return;
            }

            if (options.Mode.Equals("Async", StringComparison.OrdinalIgnoreCase))
            {
                asyncRegistration(services, options);
                return;
            }

            throw new ArgumentException($"Unsupported communication mode: {options.Mode}");
        }
    }
}
