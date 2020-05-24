﻿using System;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace App2.WebApi.Controllers
{
    [ApiController]
    [Route("dummy")]
    public class DummyController : ControllerBase
    {
        private readonly ILogger<DummyController> _logger;
       

        public DummyController(
            ILogger<DummyController> logger)
        {
            _logger = logger;

        }

        [HttpGet]
        public string Get()
        {
            Console.WriteLine(JsonSerializer.Serialize(Activity.Current));
            return "You call a dummy endpoint";
        }
    }
}