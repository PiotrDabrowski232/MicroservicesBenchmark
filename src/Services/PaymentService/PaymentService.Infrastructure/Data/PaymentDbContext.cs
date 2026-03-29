using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
namespace PaymentService.Infrastructure.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    public DbSet<TransactionRecord> Transactions { get; set; }
}
