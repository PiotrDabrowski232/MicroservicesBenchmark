using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Dependencies
{
    public static class DependencyInjections
    {
        public static IServiceCollection InjectInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var connectionstring = config.GetConnectionString("Postgres");
            services.AddDbContext<Database.ApplicationDBContext>(options => options.UseNpgsql(connectionstring));

            return services;
        }
    }
}
