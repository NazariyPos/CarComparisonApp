using System;
using CarComparisonApi.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CarComparisonApi.Controllers
{
    /// <summary>
    /// Provides a simple API health-check endpoint.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Returns basic service status message and current timestamp.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(Summary = "Health-check endpoint")]
        [ProducesResponseType(typeof(TestResponse), StatusCodes.Status200OK)]
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
