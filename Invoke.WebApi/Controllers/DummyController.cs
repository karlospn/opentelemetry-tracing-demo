using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Invoke.WebApi.Controllers
{
    [ApiController]
    [Route("dummy")]
    public class DummyController : ControllerBase
    {
        private readonly ILogger<DummyController> _logger;


        public DummyController(ILogger<DummyController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            return "You call a dummy endpoint";
        }
    }
}
