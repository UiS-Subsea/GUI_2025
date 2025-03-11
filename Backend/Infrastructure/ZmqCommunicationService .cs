using System.Text.Json;
using Backend.Infrastructure.Interface;
using NetMQ;
using NetMQ.Sockets;

public class ZmqCommunicationService : IHostedService
{
    private readonly ILogger<ZmqCommunicationService> _logger;
    private readonly ICommandQueueService<Dictionary<string, object>> _commandQueue;
    private PullSocket? _rovDataReceiver;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _rovDataListeningTask;

    private const string RovDataReceiverAddress = "tcp://127.0.0.1:5006";

    public ZmqCommunicationService(ICommandQueueService<Dictionary<string, object>> commandQueue, ILogger<ZmqCommunicationService> logger)
    {
        _logger = logger;
        _commandQueue = commandQueue;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting ZeroMQ Communication Service...");

        // Only PULL socket to receive ROV data
        _rovDataReceiver = new PullSocket();
        _rovDataReceiver.Bind(RovDataReceiverAddress);

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _rovDataListeningTask = Task.Run(() => ListenForRovDataAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

        return Task.CompletedTask;
    }

    private async Task ListenForRovDataAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listening for ROV data...");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
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
                            _logger.LogInformation($"Parsed autonom_data: X={payload[0]}, Y={payload[1]}, Z={payload[2]}, Rotation={payload[3]}");

                            var command = new Dictionary<string, object>
                            {
                                { "autonom_data", payload.Take(4).ToArray() }
                            };

                            var enqueued = await _commandQueue.EnqueueAsync(command, cancellationToken);
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

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while receiving ROV data.");
        }
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping ZeroMQ Communication Service...");

        _cancellationTokenSource?.Cancel();
        _rovDataListeningTask?.Wait();

        _rovDataReceiver?.Dispose();

        return Task.CompletedTask;
    }
    public class RovDataMessage
    {
        public string Type { get; set; } = string.Empty;
        public List<int> Payload { get; set; } = new();
    }

}
