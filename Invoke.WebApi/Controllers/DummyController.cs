using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace Invoke.WebApi.Controllers
{
    [ApiController]
    [Route("dummy")]
    public class DummyController : ControllerBase
    {
        private readonly ILogger<DummyController> _logger;
       

        public DummyController(
            ILogger<DummyController> logger,
            IServiceProvider serviceProvider
            )
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
