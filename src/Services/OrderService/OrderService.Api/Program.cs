using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Data;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Repositories;
using OrderService.Infrastructure.HttpClients;
using OrderService.Application.Orders.Commands;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OrderDbContext>(options =>
{
    var serverVersion = new MySqlServerVersion(new Version(8, 2, 0));
    options.UseMySql(connectionString, serverVersion);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderSyncCommand).Assembly));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddHttpClient<IInventoryClient, HttpInventoryClient>(client =>
{
    client.BaseAddress = new Uri("http://inventory-service:8080");
});

builder.Services.AddHttpClient<IPaymentClient, HttpPaymentClient>(client =>
{
    client.BaseAddress = new Uri("http://payment-service:8080");
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<OrderDbContext>();

        await dbContext.Database.EnsureCreatedAsync();
        await OrderDbContextSeed.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Wystąpił błąd podczas tworzenia bazy danych.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
