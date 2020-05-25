using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Exceptions;


namespace App4.RabbitConsumer.HostedService
{
    public class ProcessRabbitQueueHostedService : IHostedService
    {
        private IConnection _connection;
        private IModel _channel;
        private AsyncEventingBasicConsumer _consumer;
        private readonly Tracer _tracer;

        public ProcessRabbitQueueHostedService(IServiceProvider sp)
        {
            var assemblyName = Assembly.GetEntryAssembly().GetName();
            var name = assemblyName.Name.ToLowerInvariant();
            var version = assemblyName.Version;

            _tracer = sp.GetService<TracerFactoryBase>()?
                .GetTracer(name, $"semver:{version.ToString()}");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var factory = new ConnectionFactory() { HostName = "localhost", DispatchConsumersAsync = true };
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _consumer = new AsyncEventingBasicConsumer(_channel);
                    _consumer.Received += ExecuteAsync;
                    _channel.BasicConsume(queue: "sample_2",
                        autoAck: false,
                        consumer: _consumer);

                    return;
                }
                catch (OperationInterruptedException ex)
                {

                    _channel?.Dispose();
                    _connection?.Dispose();

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                    }
                }

            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _channel.BasicCancel(_consumer.ConsumerTags[0]);
            _channel.Close();
            _connection.Close();

            return Task.CompletedTask;
        }

        private async Task ExecuteAsync(object sender, BasicDeliverEventArgs @event)
        {
            TelemetrySpan span = null;

            try
            {
                var activity = new Activity("Process RabbitMq message");

                if (@event.BasicProperties?.Headers != null &&
                    @event.BasicProperties.Headers.TryGetValue("traceparent", out var rawTraceParent) &&
                    rawTraceParent is byte[] binRawTraceParent)
                {
                    activity.SetParentId(Encoding.UTF8.GetString(binRawTraceParent));
                }

                if (_tracer != null)
                {
                    activity.Start();
                    _tracer.StartActiveSpanFromActivity(activity.OperationName, activity, SpanKind.Consumer, out span);
                    span.SetAttribute("queue", "sample_2");
                }


                var message = Encoding.UTF8.GetString(@event.Body.Span);
               
                Console.WriteLine(" Message Received: " + message);

          
                _channel.BasicAck(deliveryTag: @event.DeliveryTag, multiple: false);


            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"There was an error: {ex.ToString()} ");
                if (span != null)
                {
                    span.SetAttribute("error", true);
                    span.Status = Status.Internal.WithDescription(ex.ToString());
                }
            }
            finally
            {
                span?.End();
            }
        }
    }
}
