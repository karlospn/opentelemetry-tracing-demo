using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


namespace App4.RabbitConsumer.HostedService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddHostedService<Worker>();

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                var connString =
                    $"{builder.Configuration["Redis:Host"]}:{builder.Configuration["Redis:Port"]}";
                options.Configuration = connString;
            });

            builder.Services.AddOpenTelemetry().WithTracing(b =>
            {

                b.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRedisInstrumentation()
                    .ConfigureRedisInstrumentation((sp, i) =>
                    {
                        var cache = (RedisCache)sp.GetRequiredService<IDistributedCache>();
                        i.AddConnection(cache.GetConnection());
                    })
                    .AddSource(nameof(Worker))
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("App4"))
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint =
                            new Uri(
                                $"{builder.Configuration["Jaeger:Protocol"]}://{builder.Configuration["Jaeger:Host"]}:{builder.Configuration["Jaeger:Port"]}");
                    });
                ;
            });

            var host = builder.Build();
            host.Run();
        }
    }
}
