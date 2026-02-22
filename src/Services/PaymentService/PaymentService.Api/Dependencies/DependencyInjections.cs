using Observability;

using PaymentService.Api.Middlewares;

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

            //TransientServices
            services.AddTransient<ExceptionHandlingMiddleware>();

            return services;
        }
    }
}
