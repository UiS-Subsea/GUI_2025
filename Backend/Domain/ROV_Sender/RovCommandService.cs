using System.Diagnostics;
using Backend.Infrastructure;
using Backend.Infrastructure.Interface;
using Backend.Translation;

namespace Backend.Domain.ROV_Sender
{
    public class RovCommandProcessor : BackgroundService
    {
        private readonly CommandQueueService<Dictionary<string, object>> _commandQueue;
        private readonly ILogger<RovCommandProcessor> _logger;
        private readonly INetworkClient _clientNetwork;
        private readonly RovTranslationLayer _rovTranslationLayer;
        private int _packetCount = 0;
        private Stopwatch _stopwatch = new Stopwatch();

        public RovCommandProcessor(CommandQueueService<Dictionary<string, object>> commandQueue, ILogger<RovCommandProcessor> logger, INetworkClient clientNetwork, RovTranslationLayer rovTranslation)
        {
            _commandQueue = commandQueue;
            _logger = logger;
            _rovTranslationLayer = rovTranslation;

            //string host = config["RovSettings:Host"] ?? "0.0.0.0";
            //int port = int.Parse(config["RovSettings:Port"] ?? "5000");
            //_network = new Network(networklogger, isServer: false, connectIP: host, port: port); // Clint MODE
            _clientNetwork = clientNetwork;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _clientNetwork.StartAsync(stoppingToken); // Start network client
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
                        if (command.TryGetValue("timestamp", out object? timestampObj) && timestampObj is DateTime timestamp)
                        {
                            TimeSpan delay = DateTime.UtcNow - timestamp;
                            _logger.LogDebug("Queue Delay: {Delay} ms", delay.TotalMilliseconds);
                        }

                        var Translatedcommand = _rovTranslationLayer.Translate(command);
                        _logger.LogInformation($"Translation: {Translatedcommand}");
                        await _clientNetwork.SendAsync(Translatedcommand, stoppingToken); // Use shared network instance

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
