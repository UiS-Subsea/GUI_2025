using Backend.Domain.Mani_Controller;
using Backend.Domain.ROV_Controller;
using Backend.Infrastructure;
using SDL2;
using System.Diagnostics;

namespace Backend
{
    public class SDL2PoolService : BackgroundService
    {
        private RovController _rovController;
        private ManiController _maniController;
        private readonly CommandQueueService<Dictionary<string, object>> _commandQueue;
        public SDL2PoolService(CommandQueueService<Dictionary<string, object>> commandQueue)
        {
            _rovController = new RovController();
            _maniController = new ManiController();
            _commandQueue = commandQueue;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SDL.SDL_SetHint(SDL.SDL_HINT_JOYSTICK_RAWINPUT, "0");

            SDL.SDL_Log("SDL Log system initialized.");

            // Initialize SDL for joystick and event handling
            SDL.SDL_Init(SDL.SDL_INIT_JOYSTICK | SDL.SDL_INIT_EVENTS);

            // Start joystick initialization
            _rovController.InitializeJoystick(); // ROV Controller.
            _maniController.InitializeJoystick(); // Mani Controller.

            // Main loop to poll events and add them to the queue
            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        SDL.SDL_Event e;
                        while (SDL.SDL_PollEvent(out e) != 0)
                        {
                            Dictionary<string, object> commandData = new Dictionary<string, object>();

                            if (_rovController.IsRelevantEvent(e))
                            {
                                _rovController.CheckJoystickConnection();

                                Dictionary<string, object> rovData = _rovController.ProcessEvents(e, stoppingToken);
                                rovData["timestamp"] = DateTime.UtcNow;  // Add timestamp
                                commandData = commandData.Concat(rovData).ToDictionary(pair => pair.Key, pair => pair.Value);
                            }

                            if (_maniController.IsRelevantEvent(e))
                            {
                                _maniController.CheckJoystickConnection();

                                Dictionary<string, object> maniData = _maniController.ProcessEvents(e, stoppingToken);
                                maniData["timestamp"] = DateTime.UtcNow;  // Add timestamp
                                commandData = commandData.Concat(maniData).ToDictionary(pair => pair.Key, pair => pair.Value);
                            }
                            if (commandData.Count > 0)  // Only enqueue if there's new data
                            {
                                Stopwatch sw = Stopwatch.StartNew();
                                await _commandQueue.EnqueueAsync(commandData);
                                sw.Stop();

                                if (sw.ElapsedMilliseconds > 1)  // Set a threshold, e.g., 10ms
                                {
                                    Console.WriteLine($"[WARNING] EnqueueAsync took too long: {sw.ElapsedMilliseconds} ms");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in event loop: {ex.Message}");
                    }

                    // You can put a small delay here if necessary (e.g., Thread.Sleep(10))
                    //Thread.Sleep(10);
                }
            }, stoppingToken);
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Shutting down...");
    
            // Close joystick if open
            if (SDL.SDL_WasInit(SDL.SDL_INIT_JOYSTICK) != 0)
            {
                _rovController.CloseJoystick();
                Console.WriteLine("Joystick closed.");
            }

            // Ensure SDL is only quit once
            if (SDL.SDL_WasInit(SDL.SDL_INIT_EVERYTHING) != 0)
            {
                SDL.SDL_Quit();
                Console.WriteLine("SDL Quit successful.");
            }
            return base.StopAsync(cancellationToken);
        }
    }
}
