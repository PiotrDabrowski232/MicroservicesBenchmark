using Messaging.Factories;

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using PaymentService.Api.GrpcServices;
using PaymentService.Application.Commands;
using PaymentService.Application.Interfaces;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.DependencyInjection;
using PaymentService.Infrastructure.Repositories;

using SharedKernel.Options;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5004, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    options.ListenAnyIP(5005, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PaymentDbContext>(options =>
{
    var serverVersion = new MySqlServerVersion(new Version(8, 2, 0));
    options.UseMySql(connectionString, serverVersion);
});

builder.Services.AddControllers();
builder.Services.AddGrpc();
builder.Services.AddOpenApi();

builder.Services.AddPaymentCommunication(builder.Configuration);

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

var serviceName = "PaymentService";
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddOtlpExporter(opts => opts.Endpoint = new Uri("http://jaeger:4317"));
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
        var dbContext = services.GetRequiredService<PaymentDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

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
        logger.LogError(ex, "Db Error");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.MapControllers();
app.MapGrpcService<PaymentGrpcService>();
app.MapPrometheusScrapingEndpoint();

app.Run();
