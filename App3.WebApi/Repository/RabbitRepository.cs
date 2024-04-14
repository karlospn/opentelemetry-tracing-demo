using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using App3.WebApi.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;

namespace App3.WebApi.Repository
{
    public class RabbitRepository(
        ILogger<RabbitRepository> logger,
        IConfiguration configuration)
        : IRabbitRepository
    {
        private static readonly ActivitySource Activity = new(nameof(RabbitRepository));
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

        public void Publish(IEvent evt)
        {
            try
            {
                using (var activity = Activity.StartActivity("RabbitMq Publish", ActivityKind.Producer))
                {
                    var factory = new ConnectionFactory { HostName = configuration["RabbitMq:Host"] };
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        var props = channel.CreateBasicProperties();

                        AddActivityToHeader(activity, props);

                        channel.QueueDeclare(queue: "sample_2",
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

                        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evt));
                        logger.LogInformation("Publishing message to queue");

                        channel.BasicPublish(exchange: "",
                            routingKey: "sample_2",
                            basicProperties: props,
                            body: body);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error trying to publish a message");
                throw;
            }

        }

        private void AddActivityToHeader(Activity activity, IBasicProperties props)
        {
            Propagator.Inject(new PropagationContext(activity.Context, Baggage.Current), props, InjectContextIntoHeader);
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination_kind", "queue");
            activity?.SetTag("messaging.rabbitmq.queue", "sample_2");
        }

        private void InjectContextIntoHeader(IBasicProperties props, string key, string value)
        {
            try
            {
                props.Headers ??= new Dictionary<string, object>();
                props.Headers[key] = value;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to inject trace context.");
            }
        }
    }
}
