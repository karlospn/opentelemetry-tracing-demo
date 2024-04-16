using System;
using App3.WebApi.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using App3.WebApi.Events;
using Microsoft.AspNetCore.Mvc;

namespace App3.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddTransient<ISqlRepository, SqlRepository>();
            builder.Services.AddTransient<IRabbitRepository, RabbitRepository>();

            builder.Services.AddControllers().AddNewtonsoftJson();
            builder.Services.AddOpenTelemetry().WithTracing(b =>
            {
                b.AddAspNetCoreInstrumentation()
                    .AddSource(nameof(RabbitRepository))
                    .AddSqlClientInstrumentation()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("App3"))
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint =
                            new Uri(
                                $"{builder.Configuration["Jaeger:Protocol"]}://{builder.Configuration["Jaeger:Host"]}:{builder.Configuration["Jaeger:Port"]}");
                    });
            });
            
            var app = builder.Build();
            
            app.MapGet("/dummy", (ILogger<Program> logger) =>
            {
                logger.LogInformation($"Logging current activity: {JsonSerializer.Serialize(Activity.Current)}");
                return "Ok";
            });

            app.MapPost("/sql-to-event", async ([FromBody] string message, 
                                                                        ISqlRepository repository, 
                                                                        IRabbitRepository eventPublisher, 
                                                                        ILogger <Program> logger) =>
            {
                logger.LogTrace("You call sql save message endpoint");
                if (!string.IsNullOrEmpty(message))
                {
                    await repository.Persist(message);
                    eventPublisher.Publish(new MessagePersistedEvent { Message = message });
                }
            });

            app.Run();
        }
    }
}