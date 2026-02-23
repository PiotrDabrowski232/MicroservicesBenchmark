using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Database
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }
    }
}
