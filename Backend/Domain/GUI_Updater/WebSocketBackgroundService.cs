using Backend.Infrastructure;

namespace Backend.Domain.GUI_Updater
{
    public class WebSocketBackgroundService : BackgroundService
    {
        private readonly WebSocketServer _webSocketServer;

        public WebSocketBackgroundService(WebSocketServer webSocketServer)
        {
            _webSocketServer = webSocketServer;
        }

        // This method runs the WebSocketServer in the background.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _webSocketServer.StartAsync(stoppingToken);
        }
    }
}
