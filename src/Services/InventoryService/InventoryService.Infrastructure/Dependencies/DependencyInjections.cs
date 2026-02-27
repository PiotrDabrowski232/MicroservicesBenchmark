using InventoryService.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryService.Infrastructure.Dependencies
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
