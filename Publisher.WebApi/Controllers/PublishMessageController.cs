using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Publisher.WebApi.Controllers
{
    [ApiController]
    [Route("publish")]
    public class PublishMessageController : ControllerBase
    {

        [HttpGet]
        public void Get()
        {
            var factory = new ConnectionFactory { HostName = "localhost"};
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "sample",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var body = Encoding.UTF8.GetBytes("I am app1");

                channel.BasicPublish(exchange: "",
                    routingKey: "sample",
                    basicProperties: null,
                    body: body);
            }
        }
    }
}
