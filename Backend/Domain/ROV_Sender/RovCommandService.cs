using System.Diagnostics;
using Backend.Infrastructure;

namespace Backend.Domain.ROV_Sender
{
    public class RovCommandProcessor : BackgroundService
    {
        private readonly CommandQueueService<Dictionary<string, object>> _commandQueue;
        private readonly ILogger<RovCommandProcessor> _logger;
        private readonly Network _network;
        private int _packetCount = 0;
        private Stopwatch _stopwatch = new Stopwatch();

        public RovCommandProcessor(CommandQueueService<Dictionary<string, object>> commandQueue, ILogger<RovCommandProcessor> logger, IConfiguration config)
        {
            _commandQueue = commandQueue;
            _logger = logger;

            string host = config["RovSettings:Host"] ?? "0.0.0.0";
            int port = int.Parse(config["RovSettings:Port"] ?? "5000");
             _network = new Network(isServer: false, connectIP: host, port: port); // Clint MODE
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _network.StartAsync(stoppingToken); // Start network client
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Network startup was canceled. Shutting down.");
                return; // Exit early if shutdown is requested
            }

            _stopwatch.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var command = await _commandQueue.DequeueAsync(stoppingToken);

                    if (command != null)
                    {
                        if (command.TryGetValue("timestamp", out object timestampObj) && timestampObj is DateTime timestamp)
                        {
                            TimeSpan delay = DateTime.UtcNow - timestamp;
                            _logger.LogDebug("Queue Delay: {Delay} ms", delay.TotalMilliseconds);
                        }

                        await _network.SendAsync(command, stoppingToken); // Use shared network instance

                        _packetCount++;
                    }
                    
                    if (_stopwatch.ElapsedMilliseconds >= 1000)
                    {
                        _logger.LogInformation("Packets per second: {PPS}", _packetCount);
                        _packetCount = 0;
                        _stopwatch.Restart();
                    }
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Command processing was canceled. Exiting...");
                    break; // Stop processing when shutdown is requested
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing command");
                    await Task.Delay(1000, stoppingToken); // Wait before retrying
                }
            }
        }
    }
}
