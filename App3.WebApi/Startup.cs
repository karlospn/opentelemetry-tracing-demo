using System;
using App3.WebApi.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("App3"))
                    .AddJaegerExporter(opts =>
                    {
                        opts.AgentHost = Configuration["Jaeger:AgentHost"];
                        opts.AgentPort = Convert.ToInt32(Configuration["Jaeger:AgentPort"]);
                    });
            });


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
                endpoints.MapGet("/health", async context =>
                {
                    await context.Response.WriteAsync("Ok");
                });
                endpoints.MapControllers();
            });
        }
    }
}
