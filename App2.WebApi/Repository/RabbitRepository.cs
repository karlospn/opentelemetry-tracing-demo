using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace App2.WebApi.Repository
{
    public class RabbitRepository : IRabbitRepository
    {
        private static readonly DiagnosticSource diagnosticSource = new DiagnosticListener("RabbitMq.Publish");


        public void Publish(IEvent evt)
        {
            Activity activity = null;
            if (diagnosticSource.IsEnabled("RabbitMq.Publish"))
            {
                activity = new Activity("Publish message to RabbitMQ");
                diagnosticSource.StartActivity(activity, null);
            }

            var factory = new ConnectionFactory { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var props = channel.CreateBasicProperties();
                if (props.Headers == null)
                {
                    props.Headers = new Dictionary<string, object>();
                }

                props.Headers.Add("traceparent", Activity.Current.Id);

                channel.ExchangeDeclare(exchange: evt.GetType().ToString(), 
                    type: ExchangeType.Fanout);

            
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));

                channel.BasicPublish(exchange: evt.GetType().ToString(),
                    routingKey: "" ,
                    basicProperties: props,
                    body: body);
            }

            if (activity != null)
            {
                diagnosticSource.StopActivity(activity, null);
            }

        }
    }
}
