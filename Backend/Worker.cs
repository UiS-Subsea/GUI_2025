
using SDL2;
using System.Collections.Concurrent;

namespace Backend
{
    public class Worker : BackgroundService
    {
        private readonly BlockingCollection<SDL.SDL_Event> _rovQueue;
        private RovController _rovController;

        public Worker(BlockingCollection<SDL.SDL_Event> queue)
        {
            _rovQueue = queue;
            _rovController = new RovController(_rovQueue);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Start joystick initialization
            _rovController.InitializeJoystick();

            // Main loop to handle joystick events and processing
            return Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Check joystick connection status (Reconnection if needed)
                    _rovController.CheckJoystickConnection();

                    // Poll and process events in the queue
                    _rovController.ProcessEvents(stoppingToken);

                    // You can add a small delay or perform other checks if necessary
                    Thread.Sleep(500);
                    //Console.WriteLine("Sleep Worker loop:");
                }
            }, stoppingToken);
        }
    }
}
