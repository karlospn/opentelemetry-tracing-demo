using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace App3.RabbitConsumer.Console
{
    public class Program
    {
        private static readonly ActivitySource Activity = new(nameof(Program));
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        public static void Main()
        {
            try
            {
                using var openTelemetry = Sdk.CreateTracerProviderBuilder()
                    .AddHttpClientInstrumentation()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("App3"))
                    .AddSource(nameof(Program))
                    .AddJaegerExporter(o =>
                    {
                        o.AgentHost = "localhost";
                        o.AgentPort = 6831;
                    })
                    .Build();

                DoWork();

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                throw;
            }
           
        }

        private static void DoWork()
        {
            var factory = new ConnectionFactory() { HostName = "localhost", DispatchConsumersAsync = true};

            var rabbitMqConnection = factory.CreateConnection();
            var rabbitMqChannel = rabbitMqConnection.CreateModel();
            var httpClient = new HttpClient {BaseAddress = new Uri("http://localhost:5001")};

            rabbitMqChannel.QueueDeclare(queue: "sample",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            rabbitMqChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(rabbitMqChannel);
            consumer.Received += async (model, ea) =>
            {
                await ProcessMessage(ea,
                    httpClient,
                    rabbitMqChannel);
            };
            
            rabbitMqChannel.BasicConsume(queue: "sample",
                autoAck: false,
                consumer: consumer);

            System.Console.WriteLine(" Press [enter] to exit.");
            System.Console.ReadLine();
        }

        private static async Task ProcessMessage(BasicDeliverEventArgs ea, 
            HttpClient httpClient, 
            IModel rabbitMqChannel)
        {
            try
            {
                var parentContext = Propagator.Extract(default, ea.BasicProperties, ExtractTraceContextFromBasicProperties);
                Baggage.Current = parentContext.Baggage;

                using (var activity = Activity.StartActivity("Process Message", ActivityKind.Consumer, parentContext.ActivityContext))
                {

                    var message = Encoding.UTF8.GetString(ea.Body.Span);
                    AddActivityTags(activity);

                    System.Console.WriteLine("Message Received: " + message);

                    _ = await httpClient.PostAsync("/sql-to-event",
                        new StringContent(JsonSerializer.Serialize(message),
                            Encoding.UTF8,
                            "application/json"));

                    rabbitMqChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }

            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"There was an error processing the message: {ex} ");
            }
        }


        private static IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties props, string key)
        {
            try
            {
                if (props.Headers.TryGetValue(key, out var value))
                {
                    var bytes = value as byte[];
                    return new[] { Encoding.UTF8.GetString(bytes) };
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Failed to extract trace context: {ex}");
            }

            return Enumerable.Empty<string>();
        }

        private static void AddActivityTags(Activity activity)
        {
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination_kind", "queue");
            activity?.SetTag("messaging.destination", "sample");
            activity?.SetTag("messaging.rabbitmq.routing_key", "sample");
        }
    }
}
