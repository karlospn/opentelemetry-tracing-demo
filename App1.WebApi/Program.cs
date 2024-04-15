using System;
using App1.WebApi.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace App1.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddOpenTelemetry().WithTracing(b =>
            {
                b.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource(nameof(PublishMessageController))
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("App1"))
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint =
                            new Uri(
                                $"{builder.Configuration["Jaeger:Protocol"]}://{builder.Configuration["Jaeger:Host"]}:{builder.Configuration["Jaeger:Port"]}");
                    });
            });

            builder.Services.AddHttpClient("app3", c =>
            {
                c.BaseAddress = new Uri(builder.Configuration["App3Endpoint"]!);
                c.Timeout = TimeSpan.FromSeconds(15);
                c.DefaultRequestHeaders.Add(
                    "accept", "application/json");
            });

            var app = builder.Build();
            app.MapControllers();
            app.Run();
        }
    }
}