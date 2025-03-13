

using Backend.Infrastructure;
using Backend.Logging;
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
        private readonly LoggerService _loggerService;

        public RovController(CommandQueueService<Dictionary<string, object>> commandQueueService, LoggerService loggerService)
        {
            _commandQueueService = commandQueueService;
            _loggerService = loggerService;
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
            // return Ok("Front light command sent successfully.");

            string status = lightCommand.Value == 2 ? "ON" : "OFF";
            _loggerService.LogInfo($"Front light turned {status}.");

            return Ok($"Front light turned {status}.");
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
            // return Ok("Bottom light command sent successfully.");

            string status = lightCommand.Value == 2 ? "ON" : "OFF";
            _loggerService.LogInfo($"Bottom light turned {status}.");

            return Ok($"Bottom light turned {status}.");
        }

    
        [HttpPost("DriveMode")]
        public async Task<IActionResult> ChangeDriveMode([FromBody] DriveModeCommand driveModeCommand)
        {
            if (driveModeCommand == null || string.IsNullOrEmpty(driveModeCommand.Mode))
            {
                Console.WriteLine("Invalid drive mode command.");
                return BadRequest("Invalid drive mode command.");
            }

            var command = new Dictionary<string, object>
            {
                { "DriveMode", driveModeCommand.Mode }
            };

            var enqueued = await _commandQueueService.EnqueueAsync(command);
            if (!enqueued)
            {
                Console.WriteLine("Failed to enqueue drive mode command.");
                return StatusCode(500, "Failed to enqueue drive mode command.");
            }

            string mode = driveModeCommand.Mode;
            _loggerService.LogInfo($"Driver mode set to {mode}."); 

            Console.WriteLine($"Drive mode changed to {driveModeCommand.Mode} successfully.");
            return Ok($"Drive mode changed to {driveModeCommand.Mode} successfully.");
        }

        private static bool? LastManipulatorStatus = null;
        [HttpPost("ManipulatorConnection")]
        public IActionResult LogManipulatorConnection([FromBody] ManipulatorConnectionCommand connectionCommand)
        {
            if (connectionCommand == null)
            {
                Console.WriteLine("Invalid Manipulator connection command.");
                return BadRequest("Invalid Manipulator connection command.");
            }

            string status = connectionCommand.IsConnected ? "connected" : "disconnected";

            if (LastManipulatorStatus == connectionCommand.IsConnected)
            {
                return Ok($"Manipulator is already {status}, skipping log.");
            }

            _loggerService.LogInfo($"Manipulator is {status}.");
            LastManipulatorStatus = connectionCommand.IsConnected;

            Console.WriteLine($"Logged Manipulator status: {status}");
            return Ok($"Manipulator is {status}.");
        }
        
        private static bool? LastRovStatus = null;
         [HttpPost("RovConnection")]
        public IActionResult LogRovConnection([FromBody] ROVConnectionCommand connectionCommand) 
        {
            if (connectionCommand == null)
            {
                Console.WriteLine("Invalid ROV connection command.");
                return BadRequest("Invalid ROV connection command.");
            }

            string status = connectionCommand.IsConnected ? "connected" : "disconnected";

            // Only log if status has changed
            if (LastRovStatus == connectionCommand.IsConnected)
            {
                return Ok($"ROV is already {status}, skipping log.");
            }

            _loggerService.LogInfo($"ROV is {status}.");
            LastRovStatus = connectionCommand.IsConnected;

            Console.WriteLine($"Logged ROV status: {status}");
            return Ok($"ROV is {status}.");
        }



    }

    public class LightCommand
    {
        public int Value { get; set; }
    }

    public class DriveModeCommand
    {
        public string Mode { get; set; }
    }

    public class ManipulatorConnectionCommand
    {
        public bool IsConnected { get; set; }
    }

   public class ROVConnectionCommand
    {
        public bool IsConnected { get; set; }
    }



}
