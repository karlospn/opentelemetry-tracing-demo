using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


namespace App4.RabbitConsumer.HostedService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    services.AddStackExchangeRedisCache(options =>
                    {
                        var connString =
                            $"{hostContext.Configuration["Redis:Host"]}:{hostContext.Configuration["Redis:Port"]}";

                        options.Configuration = connString ;
                    });

                    services.AddOpenTelemetryTracing((sp, builder) =>
                    {
                        RedisCache cache = (RedisCache)sp.GetRequiredService<IDistributedCache>();
                        builder.AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            .AddXRayTraceId()
                            .AddRedisInstrumentation(cache.GetConnectionAsync())
                            .AddSource(nameof(Worker))
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("App4"))
                            .AddOtlpExporter(opts =>
                            {
                                opts.Endpoint = new Uri(
                                    Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ??
                                    "localhost:4317");
                            });
                    });

                });
    }
}
