using System;
using Microsoft.AspNetCore.Mvc;

namespace App2.WebApi.Controllers
{
    [ApiController]
    [Route("sql/save")]
    public class MessageToSqlController : ControllerBase
    {

        [HttpGet]
        public void Get()
        {
           Console.WriteLine("You call another dummy endpoint");
        }
    }
}
