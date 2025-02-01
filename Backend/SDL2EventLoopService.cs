using SDL2;
using System.Collections.Concurrent;

namespace Backend
{
    public class SDL2EventLoopService : BackgroundService
    {
        private BlockingCollection<SDL.SDL_Event> _rovQueue;
        private IntPtr rovjoystick;

        public SDL2EventLoopService(BlockingCollection<SDL.SDL_Event> queue)
        {
            _rovQueue = queue;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SDL.SDL_SetHint(SDL.SDL_HINT_JOYSTICK_RAWINPUT, "0");
            //SDL.SDL_SetHint(SDL.SDL_HINT_JOYSTICK_HIDAPI, "1");
            //SDL.SDL_SetHint(SDL.SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS,"1");
            //SDL.SDL_SetHint(SDL.SDL_HINT_JOYSTICK_HIDAPI_XBOX, "1");
            //SDL.SDL_SetHint("SDL_JOYSTICK_HIDAPI_XBOX_ONE", "1");
            SDL.SDL_Log("SDL Log system initialized.");
            //SDL.SDL_JoystickEventState(SDL.SDL_ENABLE); // Enable joystick events

            // Initialize SDL for joystick and event handling
            SDL.SDL_Init(SDL.SDL_INIT_JOYSTICK | SDL.SDL_INIT_EVENTS);

            //rovjoystick = SDL.SDL_JoystickOpen(0);
            //SDL.SDL_GameControllerOpen(0);


            // Main loop to poll events and add them to the queue
            return Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        //SDL.SDL_JoystickUpdate();
                        //Console.WriteLine("lets check pool:");
                        SDL.SDL_Event e;
                        while (SDL.SDL_PollEvent(out e) != 0)
                        {
                            // Add event to the blocking queue
                            //SDL.SDL_Log($"Event type: {e.type}");
                            _rovQueue.Add(e, stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in event loop: {ex.Message}");
                    }

                    // You can put a small delay here if necessary (e.g., Thread.Sleep(10))
                    Thread.Sleep(500);
                    //Console.WriteLine("Sleep SDL2 event loop:");
                }
            }, stoppingToken);
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Shutting down...");
            // Perform any cleanup, like closing SDL or stopping joystick events
            SDL.SDL_Quit();
            return base.StopAsync(cancellationToken);
        }
    }
}
