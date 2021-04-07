using System.Threading.Tasks;
using App3.WebApi.Events;
using App3.WebApi.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace App3.WebApi.Controllers
{
    [ApiController]
    [Route("sql-to-event")]
    public class MessageToSqlController : ControllerBase
    {
        private readonly ISqlRepository _repository;
        private readonly IRabbitRepository _eventPublisher;
        private readonly ILogger<MessageToSqlController> _logger;

        public MessageToSqlController(ISqlRepository repository, 
            IRabbitRepository eventPublisher, 
            ILogger<MessageToSqlController> logger)
        {
            _repository = repository;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        [HttpPost]
        public async Task PostMessage([FromBody]string message)
        {
          _logger.LogTrace("You call sql save message endpoint");
           if (!string.IsNullOrEmpty(message))
           {
               await _repository.Persist(message);
               _eventPublisher.Publish(new MessagePersistedEvent {Message = message});
           }

        }
    }
}
