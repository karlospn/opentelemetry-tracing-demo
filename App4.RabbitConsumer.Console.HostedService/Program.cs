using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter.Jaeger;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Trace.Samplers;

namespace App4.RabbitConsumer.HostedService
{
    class Program
    {
        public static void Main(string[] args)
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ProcessRabbitQueueHostedService>();
                    services.AddOpenTelemetry((sp, builder) =>
                    {
                        var jaegerOptions = sp.GetService<IOptions<JaegerExporterOptions>>();
                        var name = Assembly.GetEntryAssembly()?
                            .GetName()
                            .ToString()
                            .ToLowerInvariant();

                        builder
                            .AddRequestAdapter()
                            .AddDependencyAdapter()
                            .SetResource(new Resource(new Dictionary<string, object>
                            {
                                { "service.name", name }
                            }))
                            .SetSampler(new AlwaysOnSampler())
                            .UseJaeger(o =>
                            {
                                o.ServiceName = name;
                                o.AgentHost = jaegerOptions.Value.AgentHost;
                                o.AgentPort = jaegerOptions.Value.AgentPort;
                                o.MaxPacketSize = jaegerOptions.Value.MaxPacketSize;
                                o.ProcessTags = jaegerOptions.Value.ProcessTags;

                            });
                    });
                });
        }
    }
}
