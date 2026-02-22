using Observability;

namespace PaymentService.Api.Dependencies
{
    public static class DependencyInjections
    {
        public static IServiceCollection WithServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            //Observability
            services.WithObservability(config, env.EnvironmentName);

            //Controllers
            services.AddOpenApi();
            services.AddControllers();

            return services;
        }
    }
}
