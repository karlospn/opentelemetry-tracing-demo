using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace App1.WebApi.Controllers
{
    [ApiController]
    [Route("http/app2")]
    public class CallApiController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;


        public CallApiController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            
        }


        [HttpGet]
        public async Task<string> Get()
        { 
            Console.WriteLine(JsonSerializer.Serialize(Activity.Current));
            var response  = await _httpClientFactory
                .CreateClient()
                .GetStringAsync("http://localhost:5001/dummy");

            return response;

        }
    }
}
