 using Backend.Logging; 
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace Backend.Domain.API
{
    [ApiController]
    [Route("api/logs")]
    public class LogsController : ControllerBase
    {
        // Declare a private variable for the logging service
        private readonly LoggerService _loggerService;

        // Constructor - Inject LoggerService (Dependency Injection)
        public LogsController(LoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        // // Define an API endpoint that handles GET requests
        [HttpGet]
        public IActionResult GetLogs([FromQuery] string filter = "today") 
        {
            try
            {
                var logs = _loggerService.GetFilteredLogs(filter);
                return Ok(new { logs }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving logs: {ex.Message}");
            }
        }
    }
}
