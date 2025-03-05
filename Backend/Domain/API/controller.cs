

using Backend.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace Backend.Domain.API
{
    [ApiController]
    [Route("api/rov")]
    public class RovController : ControllerBase
    {
        private readonly CommandQueueService<Dictionary<string, object>> _commandQueueService;

        public RovController(CommandQueueService<Dictionary<string, object>> commandQueueService)
        {
            _commandQueueService = commandQueueService;
        }

        [HttpPost("Front_Light_On")]
        public async Task<IActionResult> FrontLightOn([FromBody] LightCommand lightCommand)
        {
            if (lightCommand == null || lightCommand.Value == 0) // Sjekk om 0 er off
            {
                Console.WriteLine("Front Light Off");
                return BadRequest("Front Light Off");
            }

            var command = new Dictionary<string, object>
            {
                { "Front_Light_On", lightCommand.Value }
            };

            var enqueued = await _commandQueueService.EnqueueAsync(command);
            if (!enqueued)
            {
                Console.WriteLine("Failed to enqueue light command.");
                return StatusCode(500, "Failed to enqueue light command.");
            }

            Console.WriteLine($"Front light command ({lightCommand.Value}) sent successfully.");
            return Ok("Front light command sent successfully.");
        }
        

        [HttpPost("Bottom_Light_On")]
        public async Task<IActionResult> ToggleBottomLight([FromBody] LightCommand lightCommand)
        {
            if (lightCommand == null || lightCommand.Value == 0) // Ensure value is valid
            {
                Console.WriteLine("Bottom Light Off.");
                return BadRequest("Bottom Light Off.");
            }

            var command = new Dictionary<string, object>
            {
                { "Bottom_Light_On", lightCommand.Value }
            };

            var enqueued = await _commandQueueService.EnqueueAsync(command);
            if (!enqueued)
            {
                Console.WriteLine("Failed to enqueue bottom light command.");
                return StatusCode(500, "Failed to enqueue bottom light command.");
            }

            Console.WriteLine($"Bottom light command ({lightCommand.Value}) sent successfully.");
            return Ok("Bottom light command sent successfully.");
        }
    }

    public class LightCommand
    {
        public int Value { get; set; }
    }
}
