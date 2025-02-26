using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Backend.Infrastructure;
using Backend.Infrastructure.Interface;
using Backend.Translation;

namespace Backend.Domain.GUI_Updater
{
    public class DataProviderService : BackgroundService
    {
        private readonly ChannelReader<byte[]> _sensorDataReader;  // Channel reader for incoming sensor data
        private readonly ILogger<DataProviderService> _logger;     // Logger for logging information and errors
        private readonly WebSocketServer _webSocketServer;          // WebSocket server instance to communicate with frontend
        private readonly INetworkServer _serverNetwork;             // Network object to manage TCP connection with the ROV
        private readonly GUITranslationLayer _gUITranslationLayer;

        public DataProviderService(ILogger<DataProviderService> logger, WebSocketServer webSocketServer, INetworkServer serverNetwork, GUITranslationLayer gUITranslationLayer)
        {
            _serverNetwork = serverNetwork;
            _sensorDataReader = _serverNetwork.SensorData;  // Get the channel to read incoming sensor data
            _logger = logger;  // Logger for logging events/errors
            _webSocketServer = webSocketServer;  // Inject WebSocket server
            _gUITranslationLayer = gUITranslationLayer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Start the network connection and ensure it's awaited
                var networkTask = _serverNetwork.StartAsync(stoppingToken);
                _logger.LogInformation("Network client started in background.");

                // Wait for network connection to establish
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_serverNetwork.IsConnected)  // Check if network is connected
                    {
                        _logger.LogInformation("Network is connected.");
                        break;
                    }

                    _logger.LogWarning("Waiting for network connection...");
                    await Task.Delay(1000, stoppingToken);  // Wait before checking again
                }
                // Start processing sensor data
                await ProcessSensorDataAsync(stoppingToken);

                // Ensure network task completes before service exits
                await networkTask;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Service is shutting down gracefully...");
            }
        }

        private async Task ProcessSensorDataAsync(CancellationToken stoppingToken)
        {

            // Continuously wait for new sensor data until service stops
            while (!stoppingToken.IsCancellationRequested && await _sensorDataReader.WaitToReadAsync(stoppingToken))
            { 
                try
                {
                    while (_sensorDataReader.TryRead(out var sensorData))  // Read sensor data from channel
                    {
                        stoppingToken.ThrowIfCancellationRequested();  // Stop processing if service is shutting down


                        string decodedMessage = Encoding.UTF8.GetString(sensorData);
                        _logger.LogInformation($"Received Raw Data: {decodedMessage}");

                        List<Object> translatedData = _gUITranslationLayer.DecodeAndTranslatePackets(sensorData);
                        if (translatedData.Count > 0)
                        {
                            _logger.LogInformation($"Sending Sensor Data to WebSocket: {translatedData[0]}");
                        }
                        var myObject = new { 
                            Type = "COMTEMP", 
                            Com_temp = Math.Round(120.0, 2) 
                        };

                        // Send data to WebSocket clients and handle cancellation correctly
                        await _webSocketServer.SendToAllClientsAsync(myObject, stoppingToken);
                    }
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Sensor data processing canceled. Stopping...");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing sensor data");
                    await Task.Delay(1000, stoppingToken);  // Wait before retrying
                }
            }
        }
    }
}
