using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using App4.RabbitConsumer.HostedService.Helpers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ZiggyCreatures.Caching.Fusion;

namespace App4.RabbitConsumer.HostedService
{
    public class Worker(
        ILogger<Worker> logger,
        IFusionCache cache,
        IConfiguration configuration)
        : BackgroundService
    {
        private static readonly ActivitySource Activity = new(nameof(Worker));
        private static readonly TraceContextPropagator Propagator = new ();

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
            StartRabbitConsumer();
            return Task.CompletedTask;
        }

        private void StartRabbitConsumer()
        {
            var factory = new ConnectionFactory() {HostName = configuration["RabbitMq:Host"], DispatchConsumersAsync = true};
            var rabbitMqConnection = factory.CreateConnection();
            var rabbitMqChannel = rabbitMqConnection.CreateModel();

            rabbitMqChannel.QueueDeclare(queue: "sample_2",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            rabbitMqChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(rabbitMqChannel);
            consumer.Received += async (model, ea) => await ProcessMessage(ea);


            rabbitMqChannel.BasicConsume(queue: "sample_2",
                autoAck: true,
                consumer: consumer);
        }

        private async Task ProcessMessage(BasicDeliverEventArgs ea)
        {
            try
            {
                var parentContext = Propagator.Extract(default, 
                    ea.BasicProperties, 
                    ActivityHelper.ExtractTraceContextFromBasicProperties);

                Baggage.Current = parentContext.Baggage;

                using (var activity = Activity.StartActivity("Process Message", ActivityKind.Consumer, parentContext.ActivityContext))
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    ActivityHelper.AddActivityTags(activity);

                    logger.LogInformation("Message Received: " + message);

                    var result = await cache.GetOrDefaultAsync("rabbit.message", string.Empty);

                    if (string.IsNullOrEmpty(result))
                    {
                        logger.LogInformation("Add item into redis cache");

                        await cache.SetAsync("rabbit.message",
                            message,
                            new FusionCacheEntryOptions
                            {
                                Duration = TimeSpan.FromSeconds(30)
                            });
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an error processing the message");
            }
        }


    }
}
