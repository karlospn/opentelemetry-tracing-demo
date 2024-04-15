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
        public async Task<IActionResult> Get()
        { 
            logger.LogInformation("Calling App3");

            var client = httpClientFactory.CreateClient("app3");
            
            var response = await client.GetAsync("dummy");

            if (response.IsSuccessStatusCode)
                return Ok(await response.Content.ReadAsStringAsync());

            return StatusCode(500);

        }
    }
}
