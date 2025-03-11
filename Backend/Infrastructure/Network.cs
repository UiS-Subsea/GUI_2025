using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Backend.Infrastructure.Interface;

namespace Backend.Infrastructure
{
    public class Network : INetworkServer, INetworkClient
    {
        private readonly Channel<byte[]> _sensorDataChannel = Channel.CreateUnbounded<byte[]>(); // Message queue
        private readonly string _connectIP;
        private readonly int _port;
        private readonly ILogger<Network> _logger;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private readonly TimeSpan _heartbeatInterval = TimeSpan.FromMilliseconds(300); // how often a heartbeat messages is sent.
        private bool _running = true;
        private bool _isStarted = false; // boolean telling is Network is started.

        // Property to check if the connection is active
        public bool IsConnected => _running && _client?.Connected == true;

        // Expose the sensor data as a ChannelReader so other services can process it
        public ChannelReader<byte[]> SensorData => _sensorDataChannel.Reader;

        public Network(ILogger<Network> logger, IConfiguration config)
        {
            _logger = logger;
            _connectIP = config["Network:IPAddress"] ?? "10.0.0.2"; // Read from config
            _port = int.TryParse(config["Network:Port"], out int port) ? port : 6900;
        }

        /// <summary>
        /// Start the client connection and begin reading data in the background.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_isStarted)
            {
                _logger.LogInformation("Starting network tasks...");
                await StartClientAsync(cancellationToken);

                // Start reading loop in background to continuously receive data
                _ = Task.Run(() => ReadLoop(cancellationToken), cancellationToken);
                // Start background task to send periodic heartbeats
                _ = Task.Run(() => HeartbeatLoop(cancellationToken), cancellationToken);
                _isStarted = true;
            }
            else
            {
                _logger.LogInformation("Network is already started.");
            }
        }

        /// <summary>
        /// Connect to the Jetson and maintain a persistent TCP connection.
        /// </summary>
        private async Task StartClientAsync(CancellationToken cancellationToken)
        {
            while (_running && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _client = new TcpClient();
                    await _client.ConnectAsync(_connectIP, _port, cancellationToken);
                    _stream = _client.GetStream();
                    _logger.LogInformation($"Connected to {_connectIP}:{_port}");

                    return; // Successfully connected, exit the loop
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Shutting down gracefully...");
                    return; // Exit cleanly
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Connection failed: {ex.Message}. Retrying in 2s...");
                    await Task.Delay(2000, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Continuously read incoming data and push it to the channel.
        /// </summary>
        private async Task ReadLoop(CancellationToken cancellationToken)
        {
            if (_stream == null)// If there is no stream, there is nothing to read so it returns.
            {
                _logger.LogWarning("ReadLoop exited: No stream available.");
                return;
            }

            byte[] buffer = new byte[1024];

            try
            {

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                        if (bytesRead == 0) break;  // Client disconnected

                        byte[] receivedBytes = new byte[bytesRead];
                        Array.Copy(buffer, receivedBytes, bytesRead); // Copy only the received data

                        await _sensorDataChannel.Writer.WriteAsync(receivedBytes, cancellationToken); // Push data to channel
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("ReadLoop shutting down due to cancellation.");
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error reading data: {ex.Message}");
                        await ReconnectAsync(cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                 _logger.LogInformation("ReadLoop exited gracefully.");
            }
        }

        /// <summary>
        /// Send data over the TCP connection.
        /// </summary>
        public async Task SendAsync<T>(T data, CancellationToken cancellationToken)
        {
            if (_stream == null || !_client!.Connected) return;

            try
                {
                // Check if the data is a List<Dictionary<int, object>>
                if (data is List<object> listData)
                {
                    // Wrap each dictionary inside "*" and Serialize the modified list.
                    var formattedData = listData.Select(dict => $"\"*\"{JsonSerializer.Serialize(dict)}\"*\"").ToList();

                    // Concat the list into one single Json String.
                    string jsonMessage = string.Concat(formattedData);
                    _logger.LogDebug("Sending Comand String: " + jsonMessage);

                    byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonMessage);
                    await _stream.WriteAsync(jsonBytes, cancellationToken); // Sends the data.
                }
                else if (data is string stringData && stringData == "\"*\"heartbeat\"*\"") // For Sending the Heartbeat
                {
                    // If it's the heartbeat string, send it directly
                    var jsonHeartbeat = JsonSerializer.Serialize(stringData);
                    byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonHeartbeat);
                    await _stream.WriteAsync(jsonBytes, cancellationToken);
                }
                else
                {
                    // Handle the case where data is not the expected type
                    _logger.LogError("SendAsync received an unexpected data type.");
                }
                }
            catch (Exception ex)
            {
                _logger.LogError($"Send error: {ex.Message}");
                await ReconnectAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Periodically send heartbeats to maintain the connection.
        /// </summary>
        private async Task HeartbeatLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(_heartbeatInterval, cancellationToken);
                        var heartbeatData = "\"*\"heartbeat\"*\"";
                        await SendAsync(heartbeatData, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("HeartbeatLoop shutting down due to cancellation.");
                        throw; // Re-throw to exit loop if needed
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Heartbeat failed.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("HeartbeatLoop exited gracefully.");
            }
        }

        /// <summary>
        /// Reconnect to the Jetson if the connection is lost.
        /// </summary>
        private async Task ReconnectAsync(CancellationToken cancellationToken)
        {
            if (_client != null)
            {
                _client.Close();
                _client.Dispose();
                _client = null;
                _stream = null;
            }

            _logger.LogInformation("Reconnecting...");
            _isStarted = false;  // Reset the flag so tasks can be restarted
            await StartClientAsync(cancellationToken);
        }

        /// <summary>
        /// Stop the network operations and close connections.
        /// </summary>
        public void Stop()
        {
            _running = false;
            _client?.Close();

            if (_client != null)
            {
                _client.Dispose();
                _client = null;
                _stream = null;
            }
        }
    }
}
