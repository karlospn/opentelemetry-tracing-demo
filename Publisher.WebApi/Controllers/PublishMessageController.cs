using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using RabbitMQ.Client;

namespace Publisher.WebApi.Controllers
{
    [ApiController]
    [Route("publish")]
    public class PublishMessageController : ControllerBase
    {
        private readonly Tracer _tracer;
        private static readonly DiagnosticSource diagnosticSource = new DiagnosticListener("RabbitMq.Publish");


        public PublishMessageController(IServiceProvider serviceProvider)
        {
            var tracerFactory = serviceProvider.GetService<TracerFactoryBase>();

            var assemblyName = Assembly.GetEntryAssembly()?.GetName();
            var name = $"{assemblyName?.Name.ToLowerInvariant()}-enqueue";
            var version = assemblyName?.Version;

            _tracer = tracerFactory?.GetTracer(name, $"semver:{version?.ToString()}");

        }

        [HttpGet]
        public void Get()
        {

            Activity activity = null;
            if (diagnosticSource.IsEnabled("RabbitMq.Publish"))
            {
                // Generates the Publishing to RabbitMQ trace
                // Only generated if there is an actual listener
                activity = new Activity("Publish message to RabbitMQ");
                diagnosticSource.StartActivity(activity, null);
            }

            var factory = new ConnectionFactory { HostName = "localhost"};
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var props = channel.CreateBasicProperties();
                if (props.Headers == null)
                {
                    props.Headers = new Dictionary<string, object>();
                }

                props.Headers.Add("traceparent", Activity.Current.Id);

                channel.QueueDeclare(queue: "sample",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var body = Encoding.UTF8.GetBytes("I am app1");

                channel.BasicPublish(exchange: "",
                    routingKey: "sample",
                    basicProperties: null,
                    body: body);
            }

            if (activity != null)
            {
                diagnosticSource.StopActivity(activity, null);
            }
        }
    }
}
