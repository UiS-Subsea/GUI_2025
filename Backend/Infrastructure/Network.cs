using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Backend.Infrastructure
{
    public class Network
    {
        private readonly bool _isServer;
        private readonly string _bindIP;
        private readonly string _connectIP;
        private readonly int _port;
        private readonly TcpListener? _listener;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private readonly TimeSpan _heartbeatInterval = TimeSpan.FromMilliseconds(300);
        private bool _running = true;

        public Network(bool isServer, string bindIP = "0.0.0.0", int port = 6900, string connectIP = "127.0.0.1")
        {
            _isServer = isServer;
            _bindIP = bindIP;
            _connectIP = connectIP;
            _port = port;

            if (_isServer)
            {
                _listener = new TcpListener(IPAddress.Parse(bindIP), port);
            }
            else
            {
                _listener = null;
            }
        }

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
            Console.WriteLine($"Server started on {_bindIP}:{_port}");

            while (_running && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener!.AcceptTcpClientAsync(cancellationToken);
                    Console.WriteLine($"New connection from {client.Client.RemoteEndPoint}");

                    _ = HandleClientAsync(client, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Server error: {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            try
            {
                using var stream = client.GetStream();
                byte[] buffer = new byte[1024];

                while (!cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                    if (bytesRead == 0) break; // Connection closed

                    string jsonMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received JSON: {jsonMessage}");

                    // The raw sensor data is simply received, and it's up to the background service to handle it.
                    // This could be sent to a queue or event handler that other parts of your app use.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client disconnected: {ex.Message}");
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
                    Console.WriteLine($"Connected to {_connectIP}:{_port}");

                    // Start background task to maintain the heartbeat and connection.
                    _ = Task.Run(() => HeartbeatLoop(cancellationToken), cancellationToken);
                    return;
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Client shutting down gracefully...");
                    return;  // Exit cleanly
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Client connection failed: {ex.Message}. Retrying in 2s...");
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
                    Console.WriteLine($"Heartbeat failed: {ex.Message}");
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
                Console.WriteLine($"Send error: {ex.Message}");
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

            Console.WriteLine("Reconnecting...");
            await StartClientAsync(cancellationToken);
        }

        // Stop the network operations
        public void Stop()
        {
            _running = false;
            _client?.Close();
            _listener?.Stop();
        }
    }
}
