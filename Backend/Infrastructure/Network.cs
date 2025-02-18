using System.Net;
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
        private readonly bool _isServer;
        private readonly string _bindIP;
        private readonly string _connectIP;
        private readonly int _port;
        private readonly TcpListener? _listener;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private readonly TimeSpan _heartbeatInterval = TimeSpan.FromMilliseconds(300);
        private bool _running = true;
        public bool IsConnected => _running && _client?.Connected == true; // Checks both running status and TCP connection
        private readonly ILogger<Network> _logger;


        public Network(ILogger<Network> logger, bool isServer, string bindIP = "0.0.0.0", int port = 6900, string connectIP = "127.0.0.1")
        {
            _isServer = isServer;
            _bindIP = bindIP;
            _connectIP = connectIP;
            _port = port;
            _logger = logger;

            if (_isServer)
            {
                _listener = new TcpListener(IPAddress.Parse(bindIP), port);
            }
            else
            {
                _listener = null;
            }
        }

        public ChannelReader<byte[]> SensorData => _sensorDataChannel.Reader; // Expose channel reader

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_isServer)
            {
                await StartServerAsync(cancellationToken);
            }
            else
            {
                await StartClientAsync(cancellationToken);
            }
        }

        private async Task StartServerAsync(CancellationToken cancellationToken)
        {
            _listener?.Start();
            _logger.LogInformation($"TcpServer started on {_bindIP}:{_port}");

            while (_running && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener!.AcceptTcpClientAsync(cancellationToken);
                    _logger.LogInformation($"New connection from {client.Client.RemoteEndPoint}");

                    _client = client; // Set the new client as the active connection
                    _stream = _client.GetStream(); // Get the network stream for communication

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await HandleClientAsync(client, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Client error: {ex.Message}");
                        }
                    }, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("TCP Server is shutting down gracefully due to cancellation.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Server error: {ex.Message}");
                }
            }

             _listener?.Stop(); // Ensure the listener is fully stopped
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            try
            {
                using var stream = client.GetStream();
                byte[] buffer = new byte[1024];

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                        if (bytesRead == 0) break;  // Client disconnected

                        byte[] receivedBytes = new byte[bytesRead];
                        Array.Copy(buffer, receivedBytes, bytesRead); // Copy only the received data
                        //string jsonMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        try
                        {
                            await _sensorDataChannel.Writer.WriteAsync(receivedBytes, cancellationToken); // Push data to channel
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error writing to channel: {ex.Message}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // This will be triggered when the cancellation token is signaled.
                        _logger.LogInformation("Client handling canceled.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing client data: {ex.Message}");
                        break;  // Break the loop on error
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Client disconnected: {ex.Message}");
            }
        }

        private async Task StartClientAsync(CancellationToken cancellationToken)
        {
            while (_running && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _client = new TcpClient();
                    await _client.ConnectAsync(_connectIP, _port, cancellationToken);
                    _stream = _client.GetStream();
                    _logger.LogInformation($"TcpClient Connected to {_connectIP}:{_port}");

                    // Start background task to maintain the heartbeat and connection.
                    _ = Task.Run(() => HeartbeatLoop(cancellationToken), cancellationToken);
                    return;
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("TcpClient shutting down gracefully...");
                    return;  // Exit cleanly
                }
                catch (Exception ex)
                {
                    _logger.LogError($"TcpClient connection failed: {ex.Message}. Retrying in 2s...");
                    await Task.Delay(2000, cancellationToken);
                }
            }
        }

        // Periodically send heartbeats to maintain the connection
        private async Task HeartbeatLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_heartbeatInterval, cancellationToken);
                    var heartbeatData = new { type = "heartbeat" };
                    await SendAsync(heartbeatData, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Heartbeat failed: {ex.Message}");
                }
            }
        }

        // Send data (commands or heartbeat) over the TCP connection
        public async Task SendAsync<T>(T data, CancellationToken cancellationToken)
        {
            if (_stream == null || !_client!.Connected) return;

            try
            {
                string jsonMessage = JsonSerializer.Serialize(data);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonMessage);
                await _stream.WriteAsync(jsonBytes, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Send error: {ex.Message}");
                await ReconnectAsync(cancellationToken);
            }
        }

        // Reconnect to the ROV if the connection is lost
        private async Task ReconnectAsync(CancellationToken cancellationToken)
        {
            if (_client != null)
            {
                _client.Close();
                _client.Dispose();
                _client = null;
                _stream = null;
            }

            _logger.LogInformation("TCP Reconnecting...");
            await StartClientAsync(cancellationToken);
        }

        // Stop the network operations
        public void Stop()
        {
            _running = false;
            _client?.Close();
            _listener?.Stop();

            if (_client != null)
            {
                _client.Close();
                _client.Dispose();
                _client = null;
                _stream = null;
            }
        }
    }
}
