using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Publisher.WebApi.Controllers
{
    [ApiController]
    [Route("call")]
    public class CallApiController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CallApiController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        [HttpGet]
        public async Task<string> Get()
        {
            var response  = await _httpClientFactory
                .CreateClient()
                .GetStringAsync("http://localhost:5001/dummy");

            return response;

        }
    }
}
