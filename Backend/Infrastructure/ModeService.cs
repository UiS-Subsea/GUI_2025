using Backend.Infrastructure.Interface;

namespace Backend.Infrastructure
{
    public class ModeService : IModeService
    {
        // The mode is stored in a private field
        private Mode _currentMode = Mode.Manual;

        // Method to switch to Manual mode
        public void SetModeToManual()
        {
            _currentMode = Mode.Manual;
            Console.WriteLine("Manual mode started");
        }

        // Method to switch to Autonomous mode
        public void SetModeToAutonomous()
        {
            _currentMode = Mode.Autonomous;
            Console.WriteLine("Auto mode started");
        }

        // Method to check if the mode is Manual
        public bool IsManual()
        {
            return _currentMode == Mode.Manual;
        }
    }
    public enum Mode
    {
        Autonomous,
        Manual
    }
}
