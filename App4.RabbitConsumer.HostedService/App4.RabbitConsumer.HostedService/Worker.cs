using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace App4.RabbitConsumer.HostedService
{
    public class Worker : BackgroundService
    {
        private static readonly ActivitySource Activity = new(nameof(Worker));
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
            StartRabbitConsumer();
            return Task.CompletedTask;
        }

        private void StartRabbitConsumer()
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};
            var rabbitMqConnection = factory.CreateConnection();
            var rabbitMqChannel = rabbitMqConnection.CreateModel();

            rabbitMqChannel.QueueDeclare(queue: "sample_2",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            rabbitMqChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(rabbitMqChannel);
            consumer.Received += (model, ea) => ProcessMessage(ea);


            rabbitMqChannel.BasicConsume(queue: "sample_2",
                autoAck: true,
                consumer: consumer);
        }

        private void ProcessMessage(BasicDeliverEventArgs ea)
        {
            try
            {
                var parentContext = Propagator.Extract(default, 
                    ea.BasicProperties, 
                    ActivityHelper.ExtractTraceContextFromBasicProperties);

                Baggage.Current = parentContext.Baggage;

                using (var activity = Activity.StartActivity("Process Message", ActivityKind.Consumer, parentContext.ActivityContext))
                {
                    var message = Encoding.UTF8.GetString(ea.Body.Span);
                    ActivityHelper.AddActivityTags(activity);
                    System.Console.WriteLine("Message Received: " + message);
                    //Do something here with the message if you want
                }

            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"There was an error processing the message: {ex} ");
            }
        }


    }
}
