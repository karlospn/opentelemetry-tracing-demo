using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
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
                    services.Configure<JaegerExporterOptions>(hostContext.Configuration.GetSection("Jaeger"));
                    services.AddOpenTelemetryTracing((sp, builder) =>
                    {
                        var name = Assembly.GetEntryAssembly()?
                            .GetName()
                            .ToString()
                            .ToLowerInvariant();


                        builder.AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            .AddSource(name)
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("App4"))
                            .AddJaegerExporter();
                    });
                });
    }
}
