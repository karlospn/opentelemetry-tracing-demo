using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer.ConsoleApp
{
    public class Program
    { 
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            
            var rabbitMqConnection = factory.CreateConnection();
            var rabbitMqChannel = rabbitMqConnection.CreateModel();

            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5001") };

            rabbitMqChannel.QueueDeclare(queue: "sample",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            rabbitMqChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(rabbitMqChannel);
            
            consumer.Received += async (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.Span);
                Console.WriteLine(" Message Received: " + message);
                await httpClient.GetAsync("/dummier");
                rabbitMqChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

            };

            rabbitMqChannel.BasicConsume(queue: "sample",
                autoAck: false,
                consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();

        }
    }
}
