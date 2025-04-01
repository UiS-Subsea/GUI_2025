using System.Text.Json;
using Backend.Infrastructure.Interface;
using NetMQ;
using NetMQ.Sockets;

public class ZmqCommunicationService : BackgroundService
{
    private readonly ILogger<ZmqCommunicationService> _logger;
    private readonly ICommandQueueService<Dictionary<string, object>> _commandQueue;
    private PullSocket? _rovDataReceiver;

    private const string RovDataReceiverAddress = "tcp://127.0.0.1:5006";

    public ZmqCommunicationService(ICommandQueueService<Dictionary<string, object>> commandQueue, ILogger<ZmqCommunicationService> logger)
    {
        _logger = logger;
        _commandQueue = commandQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting ZeroMQ Communication Service...");

        // Only PULL socket to receive ROV data
        _rovDataReceiver = new PullSocket();
        _rovDataReceiver.Bind(RovDataReceiverAddress);

        _logger.LogInformation("Listening for ROV data...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_rovDataReceiver.TryReceiveFrameString(out string message))
                {
                    _logger.LogInformation($"[ROV DATA] Received raw: {message}");

                    try
                    {
                        // Deserialize JSON into structured format
                        var dataDict = JsonSerializer.Deserialize<Dictionary<string, List<int>>>(message);

                        if (dataDict != null && dataDict.TryGetValue("autonom_data", out var payload) && payload.Count >= 4)
                        {

                            var command = new Dictionary<string, object>
                            {
                                { "autonom_data", payload.Take(4).ToArray() }
                            };

                            var enqueued = await _commandQueue.EnqueueAsync(command, stoppingToken);
                            if (!enqueued)
                            {
                                _logger.LogWarning("Failed to enqueue autonom_data command.");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Received unknown or invalid message structure: {message}");
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"JSON Parsing Error: {ex.Message}");
                    }
                }
                else
                {
                    // Optional: Add delay to prevent busy-waiting
                    await Task.Delay(100, stoppingToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while receiving ROV data.");
        }
        finally
        {
            _rovDataReceiver?.Dispose();
        }
    }
}
