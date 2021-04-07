using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace App2.RabbitConsumer.Console
{
    public class Program
    {
        private static readonly ActivitySource Activity = new(nameof(Program));
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        private static IConfiguration _configuration;
        private static ILogger<Program> _logger;

        public static void Main()
        {
            try
            {

                SetupConfiguration();
                SetupLogger();
                using var openTelemetry = SetupOpenTelemetry();
                DoWork();

                System.Console.WriteLine(" Press [enter] to exit.");
                System.Console.ReadLine();

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                throw;
            }
           
        }

        public static void DoWork()
        {
            var factory = new ConnectionFactory() { HostName = _configuration["RabbitMq:Host"], DispatchConsumersAsync = true };

            var rabbitMqConnection = factory.CreateConnection();
            var rabbitMqChannel = rabbitMqConnection.CreateModel();
            var httpClient = new HttpClient { BaseAddress = new Uri(_configuration["App3UriEndpoint"]) };

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

                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    AddActivityTags(activity);

                    _logger.LogInformation("Message Received: " + message);

                    _ = await httpClient.PostAsync("/sql-to-event",
                        new StringContent(JsonSerializer.Serialize(message),
                            Encoding.UTF8,
                            "application/json"));

                    rabbitMqChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"There was an error processing the message: {ex} ");
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
                _logger.LogError($"Failed to extract trace context: {ex}");
            }

            return Enumerable.Empty<string>();
        }

        private static void AddActivityTags(Activity activity)
        {
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination_kind", "queue");
            activity?.SetTag("messaging.rabbitmq.queue", "sample");
        }


        private static void SetupConfiguration()
        {

            //setup config
            var configFiles = Directory
                .GetFiles(Path.Combine(Directory.GetCurrentDirectory()),
                    "appsettings.json").ToList();

            if (!configFiles.Any())
                throw new Exception("Cannot read config file");

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFiles[0], true, false)
                .AddEnvironmentVariables()
                .Build();
        }

        private static void SetupLogger()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });

            _logger = loggerFactory.CreateLogger<Program>();
        }

        private static TracerProvider SetupOpenTelemetry()
        {
            return Sdk.CreateTracerProviderBuilder()
                .AddHttpClientInstrumentation()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("App2"))
                .AddSource(nameof(Program))
                .AddJaegerExporter(opts =>
                {
                    opts.AgentHost = _configuration["Jaeger:AgentHost"];
                    opts.AgentPort = Convert.ToInt32(_configuration["Jaeger:AgentPort"]);
                    opts.ExportProcessorType = ExportProcessorType.Simple;
                })
                .Build();
        }

    }
}

