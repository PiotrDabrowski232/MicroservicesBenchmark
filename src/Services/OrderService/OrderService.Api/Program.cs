using Messaging.Factories;

using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using OrderService.Application.Interfaces;
using OrderService.Application.Orders.Commands;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.DependencyInjection;
using OrderService.Infrastructure.Repositories;

using SharedKernel.Options;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OrderDbContext>(options =>
{
    var serverVersion = new MySqlServerVersion(new Version(8, 2, 0));
    options.UseMySql(connectionString, serverVersion);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddOrderCommunication(builder.Configuration);

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

        var communicationOptions = builder.Configuration
            .GetSection("Communication")
            .Get<CommunicationOptions>()
            ?? throw new InvalidOperationException("Communication options are missing.");

        var connections = builder.Configuration
            .GetSection("ConnectionStrings")
            .Get<Dictionary<string, string>>()
            ?? throw new InvalidOperationException("Connection strings are missing.");

        if (communicationOptions.AsyncProvider.Equals("Kafka", StringComparison.OrdinalIgnoreCase))
        {
            await MessageBusFactory.InitializeAsync(
                communicationOptions.AsyncProvider,
                communicationOptions.Messaging,
                connections);
        }
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
