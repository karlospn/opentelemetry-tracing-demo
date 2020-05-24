using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter.Jaeger;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Trace.Samplers;

namespace App2.WebApi
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
            services.AddControllers();
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
