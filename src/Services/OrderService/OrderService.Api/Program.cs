using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using OrderService.Application.Interfaces;
using OrderService.Application.Orders.Commands;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.HttpClients;
using OrderService.Infrastructure.Repositories;

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

var communicationProtocol = builder.Configuration["CommunicationProtocol"] ?? "gRPC";

if (communicationProtocol.Equals("gRPC", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("STARTING ORDER SERVICE WITH: gRPC PROTOCOL");
    builder.Services.AddScoped<IInventoryClient, GrpcInventoryClient>();
    builder.Services.AddScoped<IPaymentClient, GrpcPaymentClient>();
}
else
{
    Console.WriteLine("STARTING ORDER SERVICE WITH: REST PROTOCOL");

    builder.Services.AddHttpClient<IInventoryClient, HttpInventoryClient>(client =>
    {
        var address = builder.Configuration["RestUrls:InventoryService"] ?? "http://inventory-service:5002";
        client.BaseAddress = new Uri(address);
    });

    builder.Services.AddHttpClient<IPaymentClient, HttpPaymentClient>(client =>
    {
        var address = builder.Configuration["RestUrls:PaymentService"] ?? "http://payment-service:5004";
        client.BaseAddress = new Uri(address);
    });
}

var serviceName = "OrderService";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation() 
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri("http://jaeger:4317");
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
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

//app.UseHttpsRedirection();

app.MapControllers();

app.MapPrometheusScrapingEndpoint();

app.Run();
