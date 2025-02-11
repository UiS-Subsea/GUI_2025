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
            // Set Hint for Joystick to disable RawInput, or else it can't read the Xbox One controller input.
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
                Stopwatch stopwatch = new Stopwatch();

                while (!stoppingToken.IsCancellationRequested)
                {

                    stopwatch.Restart(); // Start measuring the loop time

                    try
                    {
                        SDL.SDL_Event e;

                        // Process all events and update state
                        while (SDL.SDL_PollEvent(out e) != 0)
                        {
                            // Checks if Event belongs to the ROV, if dose then process it.
                            if (_rovController.IsRelevantEvent(e))
                            {
                                _rovController.CheckJoystickConnection();
                                // Process the Event and stores data internally in the ROVController.
                                _rovController.ProcessEvents(e, stoppingToken);
                            }

                            // Checks if Event belongs to the Manipulator, if dose then process it.
                            if (_maniController.IsRelevantEvent(e))
                            {
                                _maniController.CheckJoystickConnection();
                                // Process the Event and stores data internally in the ManiController.
                                _maniController.ProcessEvents(e, stoppingToken);
                            }
                        }

                        // Get final data at the end of the tick
                        Dictionary<string, object> rovData = _rovController.GetState();
                        Dictionary<string, object> maniData = _maniController.GetState();

                        // Merge both datasets
                        Dictionary<string, object> commandData = rovData.Concat(maniData)
                            .ToDictionary(pair => pair.Key, pair => pair.Value);

                        // Used to check for Queue Delay.
                        commandData["timestamp"] = DateTime.UtcNow;  // Add timestamp

                        // Send the final merged data only if there's something to send
                        if (commandData.Count > 1) // More than just timestamp
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
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in event loop: {ex.Message}");
                    }

                    // Ensure the loop runs exactly 20 times per second
                    int elapsedMs = (int)stopwatch.ElapsedMilliseconds;
                    int delay = Math.Max(50 - elapsedMs, 0); // Adjust delay to maintain 20 Hz

                    await Task.Delay(delay, stoppingToken);
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
                _maniController.CloseJoystick();
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
