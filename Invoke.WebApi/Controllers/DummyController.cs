using System;
using Microsoft.AspNetCore.Mvc;

namespace Invoke.WebApi.Controllers
{
    [ApiController]
    [Route("Invoke")]
    public class DummyController : ControllerBase
    {

        [HttpGet]
        public void Get()
        {
           Console.WriteLine("You call my api");
        }
    }
}
