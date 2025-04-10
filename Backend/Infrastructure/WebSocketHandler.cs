using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using Backend.Infrastructure.Interface;

namespace Backend.Infrastructure
{
    public class WebSocketServer
    {
        // Stores connected clients using a thread-safe dictionary.
        // The dictionary key is the WebSocket instance, and the value is a placeholder byte (not used).
        private readonly ConcurrentDictionary<WebSocket, byte> _connectedClients = new();
        private readonly HttpListener _httpListener; // HTTP listener to accept WebSocket requests
        private readonly int _port;
        private readonly ILogger<WebSocketServer> _logger;
        private readonly IModeService _modeService;
        private readonly List<string> _pendingMessages = new List<string>();


        /// <summary>
        /// Initializes a new WebSocket server instance on the specified port.
        /// </summary>
        /// <param name="port">The port on which the server should listen for WebSocket connections.</param>
        public WebSocketServer(ILogger<WebSocketServer> logger, IModeService modeService, int port = 5009)
        {
            _port = port;
            _httpListener = new HttpListener();
            _logger = logger;
            _modeService = modeService;

            // WebSocket connections will be accepted at ws://localhost:{port}/ws/
            _httpListener.Prefixes.Add($"http://localhost:{_port}/ws/");
        }

        /// <summary>
        /// Starts the WebSocket server and listens for incoming connections.
        /// </summary>
        /// <param name="cancellationToken">Token to signal server shutdown.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _httpListener.Start(); // Start the HTTP listener
            _logger.LogInformation($"WebSocket server started on ws://localhost:{_port}/ws");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Wait for either a new connection or cancellation.
                    var contextTask = _httpListener.GetContextAsync();
                    var timeoutTask = Task.Delay(Timeout.Infinite, cancellationToken); // Wait indefinitely for cancellation
                    var completedTask = await Task.WhenAny(contextTask, timeoutTask); // Wait for either of them to complete

                    if (completedTask == contextTask)
                    {
                        var context = await contextTask; // The incoming WebSocket request.

                        if (context.Request.IsWebSocketRequest) // Checks if it is a Websocket request.
                        {
                            var wsContext = await context.AcceptWebSocketAsync(null);
                            var webSocket = wsContext.WebSocket;
                            _logger.LogInformation("WebSocket Client connected!");

                            // Add the client WebSocket to the dictionary
                            _connectedClients.TryAdd(webSocket, 0);

                            foreach (var pendingMessage in _pendingMessages)
                            {
                                await SendMessageToClient(webSocket, pendingMessage, cancellationToken);
                            }

                            // Clear the pending message list once they've been sent
                            _pendingMessages.Clear();

                            // Handle the client communication in a separate task
                            _ = HandleClientAsync(webSocket, cancellationToken);
                        }
                        else
                        {
                            // Reject non-WebSocket requests with a 400 (Bad Request) response
                            context.Response.StatusCode = 400;
                            context.Response.Close();
                        }
                    }
                    else
                    {
                        // Timeout (or cancellation) has occurred, exit the loop
                        break;
                    }
                }
            }
            catch (Exception ex) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogError("WebSocket Server Error:" + ex.Message, ex);
                _logger.LogInformation("WebSocket Server shutting down...");
            }
            finally
            {
                await ShutdownAsync(cancellationToken); // Ensure graceful shutdown
            }
        }


        /// <summary>
        /// Handles communication with an individual WebSocket client.
        /// </summary>
        /// <param name="socket">The WebSocket connection with the client.</param>
        /// <param name="cancellationToken">Token to signal shutdown.</param>
        private async Task HandleClientAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            var buffer = new byte[1024 * 4]; // Buffer for receiving data

            try
            {
                while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    // Wait for a message from the client
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // Client has requested to close the connection
                        _logger.LogInformation("Websocket Client disconnected.");
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", cancellationToken);
                        _connectedClients.TryRemove(socket, out _);
                        return;
                    }

                    // Convert received bytes into a string message
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.LogInformation($"Received: {receivedMessage}");

                    // Deserialize the received JSON string into an object
                    try
                    {
                        var messageObject = JsonSerializer.Deserialize<Dictionary<string, string>>(receivedMessage);

                        // Check if the Mode key exists and handle accordingly
                        if (messageObject != null && messageObject.ContainsKey("Mode"))
                        {
                            string mode = messageObject["Mode"];

                            if (mode == "MANUAL") // 0 for Manual
                            {
                                _modeService.SetModeToManual();
                            }
                            else if (mode == "AUTO") // 1 for Autonomous
                            {
                                _modeService.SetModeToAutonomous();
                            }
                            else
                            {
                                _logger.LogWarning($"Unrecognized mode value: {mode}");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Received message does not contain 'Mode' key.");
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"Error deserializing message: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling WebSocket client: {ex.Message}");
            }
            finally
            {
                // Ensure the client is removed and the connection is properly closed
                _connectedClients.TryRemove(socket, out _);

                if (socket.State != WebSocketState.Closed)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error occurred", cancellationToken);
                }
            }
        }
        /// <summary>
        /// Sends a message to a specific WebSocket client.
        /// </summary>
        /// <param name="socket">The WebSocket connection to the client.</param>
        /// <param name="message">The message to send, serialized as a string.</param>
        /// <param name="cancellationToken">A cancellation token to signal the task cancellation.</param>
        /// <returns>A task representing the asynchronous operation of sending the message.</returns>
        private async Task SendMessageToClient(WebSocket socket, string message, CancellationToken cancellationToken)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, cancellationToken);
        }

        /// <summary>
        /// Sends a message to all connected WebSocket clients.
        /// </summary>
        /// <typeparam name="T">Type of message being sent (will be serialized to JSON).</typeparam>
        /// <param name="message">The message object to send.</param>
        /// <param name="ensureDelivery">
        /// If true, the message is stored and sent later if no clients are currently connected.
        /// Otherwise, the message is discarded if no clients are connected.
        /// </param>
        /// <param name="cancellationToken">Token to cancel sending if needed.</param>
        public async Task SendToAllClientsAsync<T>(T message, CancellationToken cancellationToken,  bool ensureDelivery = false)
        {
            string json = JsonSerializer.Serialize(message); // Serialize the message into JSON
            if (ensureDelivery && _connectedClients.IsEmpty)
            {
                _pendingMessages.Add(json);
            }
            else
            {
                byte[] data = Encoding.UTF8.GetBytes(json); // Convert JSON to bytes

                // Iterate over connected clients safely
                foreach (var socket in _connectedClients.Keys.ToList()) // Copy to avoid modification issues
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        // Send the message to the client
                        await socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, cancellationToken);
                    }
                    else
                    {
                        // Remove disconnected clients
                        _connectedClients.TryRemove(socket, out _);
                    }
                }
            }
        }

        /// <summary>
        /// Gracefully shuts down the WebSocket server, closing all client connections.
        /// </summary>
        public async Task ShutdownAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Shutting down WebSocket server...");

            // Stop listening for new incoming WebSocket connections
            _httpListener.Stop();  // Stops the HTTP listener, preventing new WebSocket clients from connecting
            _httpListener.Close(); // Closes the listener completely, freeing up the port

            // Close all active WebSocket client connections gracefully
            foreach (var socket in _connectedClients.Keys.ToList()) // Copy list to avoid modification issues during iteration
            {
                try
                {
                    if (socket.State == WebSocketState.Open) // Ensure socket is still connected
                    {
                        // Attempt to close the WebSocket gracefully
                        var contextTask = socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", cancellationToken);
                        var timeoutTask = Task.Delay(Timeout.Infinite, cancellationToken); // Wait indefinitely for cancellation
                        var completedTask = await Task.WhenAny(contextTask, timeoutTask); // Wait for either of them to complete
                    }
                }
                catch (OperationCanceledException)
                {
                    // If the CloseAsync operation takes too long, forcefully abort the WebSocket
                    _logger.LogWarning("WebSocket CloseAsync timed out, forcefully aborting...");
                    socket.Abort(); // Immediately terminate the WebSocket connection
                }
                catch (Exception ex)
                {
                    // Log any unexpected errors while closing the socket
                    _logger.LogError($"Error closing client socket: {ex.Message}");
                }

                // Remove the WebSocket from the dictionary, regardless of whether it was closed successfully or not
                _connectedClients.TryRemove(socket, out _);
            }

            _logger.LogInformation("WebSocket server shut down successfully.");
        }
    }
}
