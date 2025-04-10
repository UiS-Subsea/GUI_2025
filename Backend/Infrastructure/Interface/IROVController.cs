
using SDL2;

namespace Backend.Infrastructure.Interface
{
    public interface IROVController
    {
        void InitializeJoystick(int index);  // Initialize the ROVController.
        bool ProcessEvents(SDL.SDL_Event e, CancellationToken cancellationToken);  // Process the Events into Packets
        bool IsRelevantEvent(SDL.SDL_Event e); //Checks if the SDL2 event belongs to the ROVController.
        Dictionary<string, object> GetState(); // Returns the Packets containing the state.
        int GetJoystickId();
        void CloseJoystick();
    }
}
