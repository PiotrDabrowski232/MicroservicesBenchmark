using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Database
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }
    }
}
