using System;
using App3.WebApi.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Contrib.Extensions.AWSXRay.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace App3.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ISqlRepository, SqlRepository>();
            services.AddTransient<IRabbitRepository, RabbitRepository>();

            services.AddControllers().AddNewtonsoftJson();
            services.AddOpenTelemetryTracing(builder =>
            {
                builder.AddAspNetCoreInstrumentation()
                    .AddSource(nameof(RabbitRepository))
                    .AddSqlClientInstrumentation()
                    .AddXRayTraceId()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("App3"))
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ??
                                                "localhost:4317");
                    });
            });

            Sdk.SetDefaultTextMapPropagator(new AWSXRayPropagator());

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
