using System;
using System.Threading.Tasks;
using App2.WebApi.Events;
using App2.WebApi.Repository;
using Microsoft.AspNetCore.Mvc;

namespace App2.WebApi.Controllers
{
    [ApiController]
    [Route("sql/event")]
    public class MessageToSqlController : ControllerBase
    {
        private readonly ISqlRepository _repository;
        private readonly IRabbitRepository _eventPublisher;

        public MessageToSqlController(ISqlRepository repository, 
            IRabbitRepository eventPublisher)
        {
            _repository = repository;
            _eventPublisher = eventPublisher;
        }

        [HttpPost]
        public async Task PostMessage([FromBody]string message)
        {
           Console.WriteLine("You call sql save message endpoint");
           if (!string.IsNullOrEmpty(message))
           {
               await _repository.Persist(message);
               _eventPublisher.Publish(new MessagePersistedEvent {Message = message});
           }

        }
    }
}
