using System.Diagnostics;
using Backend.Infrastructure;
using Backend.Infrastructure.Interface;
using Backend.Translation;

namespace Backend.Domain.ROV_Sender
{
    public class RovCommandProcessor : BackgroundService
    {
        private readonly ICommandQueueService<Dictionary<string, object>> _commandQueue;
        private readonly ILogger<RovCommandProcessor> _logger;
        private readonly INetworkClient _clientNetwork;
        private readonly IRovTranslationLayer _rovTranslationLayer;
        private int _packetCount = 0; // For Debugging to monitoring Packets per second.

        public RovCommandProcessor(ICommandQueueService<Dictionary<string, object>> commandQueue, ILogger<RovCommandProcessor> logger, INetworkClient clientNetwork, IRovTranslationLayer rovTranslation)
        {
            _commandQueue = commandQueue; //Internal Queue containing the generic commands from other services.
            _logger = logger;
            _rovTranslationLayer = rovTranslation; // Translate Generic commands to ROV specific commands.
            _clientNetwork = clientNetwork; // The network class in CLient mode (for sending)
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

            var lastCheck = DateTime.UtcNow;

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

                        var ROVData = _rovTranslationLayer.Translate(command);
                        _logger.LogDebug($"Translation: {ROVData}");
                        await _clientNetwork.SendAsync(ROVData, stoppingToken); // Use shared network instance

                        _packetCount++;
                    }
                    
                    // Check packets per second every second
                    var now = DateTime.UtcNow;
                    if ((now - lastCheck).TotalMilliseconds >= 1000)
                    {
                        _logger.LogDebug("Packets per second: {PPS}", _packetCount);
                        _packetCount = 0;
                        lastCheck = now;
                    }
                }
                catch (OperationCanceledException)
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
