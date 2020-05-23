using System;
using Microsoft.AspNetCore.Mvc;

namespace Invoke.WebApi.Controllers
{
    [ApiController]
    [Route("dummier")]
    public class AnotherDummyController : ControllerBase
    {

        [HttpGet]
        public void Get()
        {
           Console.WriteLine("You call another dummy endpoint");
        }
    }
}
