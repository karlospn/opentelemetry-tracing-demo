using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter.Jaeger;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Trace.Samplers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer.ConsoleApp
{
    public class Program
    { 
        public static void Main(string[] args)
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

            var serviceProvider = new ServiceCollection().
                AddOpenTelemetry((sp, builder) =>
                {
                    var name = Assembly.GetEntryAssembly()?
                        .GetName()
                        .ToString()
                        .ToLowerInvariant();

                    builder
                        .AddRequestAdapter()
                        .AddDependencyAdapter()
                        .SetResource(new Resource(new Dictionary<string, object>
                        {
                            { "service.name", name },
                            { "Description", "consumer console app" }
                        }))
                        .SetSampler(new AlwaysOnSampler())
                        .UseJaeger(o =>
                        {
                            o.ServiceName = name;
                            o.AgentHost = "localhost";
                            o.AgentPort = 6831;
                            o.MaxPacketSize = 65000;

                        });
                })
                .BuildServiceProvider();

            DoWork(serviceProvider);
        }

        private static void DoWork(IServiceProvider sp)
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};

            var rabbitMqConnection = factory.CreateConnection();
            var rabbitMqChannel = rabbitMqConnection.CreateModel();

            var httpClient = new HttpClient {BaseAddress = new Uri("http://localhost:5001")};
            
            var assemblyName = Assembly.GetEntryAssembly().GetName();
            var name = assemblyName.Name.ToLowerInvariant();
            var version = assemblyName.Version;
            
            var tracer = sp.GetService<TracerFactoryBase>()?.GetTracer(name, $"semver:{version.ToString()}");

            rabbitMqChannel.QueueDeclare(queue: "sample",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            rabbitMqChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(rabbitMqChannel);

            consumer.Received += async (model, ea) =>
            {
                await ProcessMessage(ea,
                    httpClient,
                    rabbitMqChannel,
                    sp,
                    tracer);
            };

            rabbitMqChannel.BasicConsume(queue: "sample",
                autoAck: false,
                consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static async Task ProcessMessage(BasicDeliverEventArgs ea, 
            HttpClient httpClient, 
            IModel rabbitMqChannel,
            IServiceProvider sp,
            Tracer tracer)
        {
            TelemetrySpan span = null;

            try
            {
                var activity = new Activity("Process RabbitMq message");

                if (ea.BasicProperties.Headers.TryGetValue("traceparent", out var rawTraceParent) &&
                    rawTraceParent is byte[] binRawTraceParent)
                {
                    activity.SetParentId(Encoding.UTF8.GetString(binRawTraceParent));
                }

                if (tracer != null)
                {
                    activity.Start();
                    tracer.StartActiveSpanFromActivity(activity.OperationName, activity, SpanKind.Consumer, out span);
                    span.SetAttribute("queue", "sample");
                }


                var message = Encoding.UTF8.GetString(ea.Body.Span);
                Console.WriteLine(" Message Received: " + message);
                
                //await httpClient.GetAsync("/dummier");
                rabbitMqChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);


            }
            catch (Exception ex)
            {
                if (span != null)
                {
                    span.SetAttribute("error", true);
                    span.Status = Status.Internal.WithDescription(ex.ToString());
                }
                throw;
            }
            finally
            {
                span?.End();
            }
        }
    }
}
