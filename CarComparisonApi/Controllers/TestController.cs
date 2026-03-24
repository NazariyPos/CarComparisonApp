using System;
using CarComparisonApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarComparisonApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var response = new TestResponse
            {
                Message = "API працює",
                Timestamp = DateTime.Now
            };
            return Ok(response);
        }
    }
}