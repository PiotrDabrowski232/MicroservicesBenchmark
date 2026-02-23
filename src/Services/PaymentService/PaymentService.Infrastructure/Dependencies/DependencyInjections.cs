using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

using PaymentService.Infrastructure.Database;

namespace PaymentService.Infrastructure.Dependencies
{
    public static class DependencyInjections
    {
        public static IServiceCollection InjectInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var connectionstring = config.GetConnectionString("Postgres");
            services.AddDbContext<ApplicationDBContext>(options => options.UseNpgsql(connectionstring));

            return services;
        }
    }
}
