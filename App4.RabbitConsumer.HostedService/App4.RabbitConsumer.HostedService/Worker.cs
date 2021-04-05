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

namespace App4.RabbitConsumer.HostedService
{
    public class Worker : BackgroundService
    {
        private static readonly ActivitySource Activity = new(nameof(Worker));
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        private readonly ILogger<Worker> _logger;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;


        public Worker(ILogger<Worker> logger, 
            IDistributedCache cache,
            IConfiguration configuration)
        {
            _logger = logger;
            _cache = cache;
            _configuration = configuration;
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
            var factory = new ConnectionFactory() {HostName = _configuration["RabbitMq:Host"], DispatchConsumersAsync = true};
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

                    _logger.LogInformation("Message Received: " + message);

                    var item = await _cache.GetStringAsync("rabbit.message");
                    if (string.IsNullOrEmpty(item))
                    {
                        _logger.LogInformation("Add item into redis cache");
                        
                        await _cache.SetStringAsync("rabbit.message", 
                            message, 
                            new DistributedCacheEntryOptions
                            {
                                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(1)
                            });
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"There was an error processing the message: {ex} ");
            }
        }


    }
}
