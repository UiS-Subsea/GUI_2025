using System.Diagnostics;
using Backend.Infrastructure;

namespace Backend.Domain.ROV_Sender
{
    public class RovCommandProcessor : BackgroundService
    {
        private readonly CommandQueueService<Dictionary<string, object>> _commandQueue;

        public RovCommandProcessor(CommandQueueService<Dictionary<string, object>> commandQueue)
        {
            _commandQueue = commandQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
                            Console.WriteLine($"[DIAGNOSTIC] Queue Delay: {delay.TotalMilliseconds} ms");
                        }

                        SendToRov(command);
                    }
                    else
                    {
                        Console.WriteLine("Not command in Queue!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing command: {ex.Message}");
                }
            }
        }

        private void SendToRov(Dictionary<string, object> command)
        {
            Console.WriteLine("Command Out of Queue:  " + string.Join(", ", command.Select(kvp => $"{kvp.Key}: {kvp.Value}")));
        }
    }
}
