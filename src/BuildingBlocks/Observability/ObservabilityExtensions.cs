using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


namespace Observability
{
    public static class ObservabilityExtensions
    {
        public static IServiceCollection WithObservability(this IServiceCollection services, IConfiguration config, string serviceName)
        {
            var otlpEndpoint = config["OTEL_EXPORTER_OTLP_ENDPOINT"];
            var resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName);

            services.AddOpenTelemetry()
                .ConfigureResource(rb => rb.AddService(serviceName))
                .WithTracing(tracing =>
                {
                    tracing
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                        .AddHttpClientInstrumentation();

                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    {
                        tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
                    }
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();

                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    {
                        metrics.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
                    }
                });

            return services;
        }
    }
}
