using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace App1.WebApi.Controllers
{
    [ApiController]
    [Route("http")]
    public class CallApiController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CallApiController> _logger;

        public CallApiController(
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration, 
            ILogger<CallApiController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public async Task<string> Get()
        { 
            _logger.LogInformation($"Calling App3: {_configuration["App3Endpoint"]}");
            var response  = await _httpClientFactory
                .CreateClient()
                .GetStringAsync(_configuration["App3Endpoint"]);

            return response;

        }
    }
}
