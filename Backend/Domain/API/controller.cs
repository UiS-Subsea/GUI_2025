

using Backend.Infrastructure;
using Backend.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Backend.Infrastructure.Interface;

namespace Backend.Domain.API
{
    [ApiController]
    [Route("api/rov")]
    public class RovController : ControllerBase
    {
        private readonly ICommandQueueService<Dictionary<string, object>> _commandQueueService;
        private readonly LoggerService _loggerService;

        public RovController(ICommandQueueService<Dictionary<string, object>> commandQueueService, LoggerService loggerService)
        {
            _commandQueueService = commandQueueService;
            _loggerService = loggerService;
        }


        [HttpPost("Front_Light_On")]
        public async Task<IActionResult> FrontLightOn([FromBody] LightCommand lightCommand)
        {
            if (lightCommand == null)
            {
                Console.WriteLine("Invalid Front Light command.");
                return BadRequest("Invalid Front Light command.");
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

            string status = lightCommand.Value == 2 ? "ON" : "OFF";
            _loggerService.LogInfo($"Front light turned {status}.");
            
            Console.WriteLine($"Front light turned {status}.");
            Console.WriteLine($"Front light Value {lightCommand.Value}.");

            return Ok($"Front light turned {status}.");
        }

        [HttpPost("Bottom_Light_On")]
        public async Task<IActionResult> ToggleBottomLight([FromBody] LightCommand lightCommand)
        {
            if (lightCommand == null)
            {
                Console.WriteLine("Invalid Bottom Light command.");
                return BadRequest("Invalid Bottom Light command.");
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

            LastRovStatus = connectionCommand.IsConnected;

            if (connectionCommand.IsConnected)
            {
                _loggerService.LogInfo($"ROV is {status}.");
            }
            else
            {
                _loggerService.LogWarning($"ROV connection lost!");
            }

            return Ok($"ROV is {status}.");
        }

        private static Dictionary<string, string> LastTemperatureStatus = new Dictionary<string, string>();
        [HttpPost("TemperatureLog")]
        public IActionResult LogTemperature([FromBody] TemperatureStatusCommand temperatureData)
        {
            if (temperatureData == null)
            {
                return BadRequest("Invalid temperature data.");
            }

            //Only log these specific sensors**
            string[] allowedSensors = { "Com Temperature", "Regulator Temperature", "Motor Temperature", "Sensor Temperature", "12V Right Temperature", "12V Left Temperature", "5V Power Temperature" };

            if (!allowedSensors.Contains(temperatureData.SensorName))
            {
                return Ok($"Skipping log for {temperatureData.SensorName} (Not in allowed sensors).");
            }

            string status;
            bool isWarning = false;

            if (temperatureData.Value >= 30)
            {
                status = $"{temperatureData.SensorName} is Too High ({temperatureData.Value}°C)";
                isWarning = true;
            }
            else if (temperatureData.Value >= 20)
            {
                status = $"{temperatureData.SensorName} is High ({temperatureData.Value}°C)";
                isWarning = true;
            }
            else
            {
                status = $"{temperatureData.SensorName} is Normal ({temperatureData.Value}°C)";
            }

            // **Only log if status has changed**
            if (LastTemperatureStatus.TryGetValue(temperatureData.SensorName, out string lastStatus) && lastStatus == status)
            {
                return Ok($"Temperature for {temperatureData.SensorName} is unchanged, skipping log.");
            }

            LastTemperatureStatus[temperatureData.SensorName] = status;

            if (isWarning)
            {
                _loggerService.LogWarning(status);
            }
            else
            {
                _loggerService.LogInfo(status);
            }


            return Ok(status);
        }

        private static string? LastBatteryStatus = null;
        [HttpPost("BatteryStatus")]
        public IActionResult LogBatteryStatus([FromBody] BatteryStatusCommand batteryCommand)
        {
            if (batteryCommand == null)
            {
                Console.WriteLine("Invalid battery command.");
                return BadRequest("Invalid battery command.");
            }

            string status = batteryCommand.Percentage switch
            {
                < 20 => "Battery LOW",
                < 60 => "Battery MEDIUM",
                _ => "Battery OK"
            };

            // Only log if status has changed
            if (LastBatteryStatus == status)
            {
                return Ok($"Battery status unchanged ({status}), skipping log.");
            }

            if (batteryCommand.Percentage < 20)
            {
                _loggerService.LogWarning(status);
            }
            else
            {
                _loggerService.LogInfo(status);
            }

            LastBatteryStatus = status; 

            return Ok($"Battery status: {status}");
        }

        [HttpPost("Transducer_Toggle")]
        public async Task<IActionResult> ToggleTransducer([FromBody] TransducerCommand transducerCommand)
        {
            if (transducerCommand == null)
            {
                Console.WriteLine("Invalid Transducer command.");
                return BadRequest("Invalid Transducer command.");
            }

            var command = new Dictionary<string, object>
            {
                { "Transducer_Toggle", transducerCommand.IsOn ? 2 : 0 }
            };

            var enqueued = await _commandQueueService.EnqueueAsync(command);
            if (!enqueued)
            {
                Console.WriteLine("Failed to enqueue transducer command.");
                return StatusCode(500, "Failed to enqueue transducer command.");
            }

            string status = transducerCommand.IsOn ? "ON" : "OFF";

            // Only log when status changes
            if (LastTransducerStatus == transducerCommand.IsOn)
            {
                return Ok($"Transducer is already {status}, skipping log.");
            }

            LastTransducerStatus = transducerCommand.IsOn;
            _loggerService.LogInfo($"Transducer turned {status}.");

            Console.WriteLine($"Transducer turned {status}.");
            return Ok($"Transducer turned {status}.");
        }

        //Get the Transducer status when frontend loads
        [HttpGet("TransducerStatus")]
        public IActionResult GetTransducerStatus()
        {
            return Ok(new { isOn = LastTransducerStatus });
        }

        private static bool LastTransducerStatus = false;
    }
}

//Data models
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

    public class TemperatureStatusCommand
    {
        public string SensorName { get; set; }
        public float Value { get; set; }
    }

    public class BatteryStatusCommand
    {
        public int Percentage { get; set; }
    }

    public class TransducerCommand
    {
        public bool IsOn { get; set; }
    }

