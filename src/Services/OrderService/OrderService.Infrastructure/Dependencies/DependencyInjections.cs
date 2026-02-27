using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

using OrderService.Infrastructure.Database;

namespace OrderService.Infrastructure.Dependencies
{
    public static class DependencyInjections
    {
        public static IServiceCollection InjectInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var connectionstring = config.GetConnectionString("Postgres");
            services.AddDbContext<ApplicationDBContext>(options =>
            {
                options.UseNpgsql(connectionstring);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            return services;
        }
    }
}
