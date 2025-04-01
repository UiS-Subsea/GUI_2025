
using SDL2;

namespace Backend.Infrastructure.Interface
{
    public interface IManiController
    {
        void InitializeJoystick();  // Initialize the ManiController.
        bool ProcessEvents(SDL.SDL_Event e, CancellationToken cancellationToken);  // Process the Events into Packets
        bool IsRelevantEvent(SDL.SDL_Event e); //Checks if the SDL2 event belongs to the ManiController.
        Dictionary<string, object> GetState(); // Returns the Packets containing the state.
        void CloseJoystick();
    }
}
