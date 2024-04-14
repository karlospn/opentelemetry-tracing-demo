using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace App1.WebApi.Controllers
{
    [ApiController]
    [Route("http")]
    public class CallApiController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<CallApiController> logger)
        : ControllerBase
    {
        [HttpGet]
        public async Task<string> Get()
        { 
            logger.LogInformation("Calling App3");
            
            var response  = await httpClientFactory
                .CreateClient()
                .GetStringAsync(configuration["App3Endpoint"]);

            return response;

        }
    }
}
