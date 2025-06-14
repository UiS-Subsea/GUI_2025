using Backend.Infrastructure.Interface;
using SDL2;

namespace Backend.Domain.Controllers.Mani_Controller
{
    public class ManiController : IManiController
    {

        private readonly ILogger<ManiController> _logger;
        private IntPtr JoystickPtr; 
        private int joystickIndex = -1;
        private int JoystickId; // Store The Xbox One Controllers InstanceID.
        public bool IsConnected => JoystickPtr != IntPtr.Zero && SDL.SDL_JoystickGetAttached(JoystickPtr) == SDL.SDL_bool.SDL_TRUE;

        private const int joystickDeadzone = 15; // The deadzone Value.
        private const float MaxValue = 32767.0f; // The Xbox One Controllers Max Joystick Value.
        private const float MinValue = -32768.0f; // The Xbox One Controllers Min Joystick Value.

        private int[] mani_buttons = new int[16]; // Store button states (0 or 1).
        private int[] mani_axis = new int[7]; // Store joystick axis states in [x, y, z, rotation, 0, 0, 0, 0].
        private (int x, int y) mani_dpad = (0, 0);

        public ManiController(ILogger<ManiController> logger)
        {
            _logger = logger;
        }

        public void InitializeJoystick(int index)
        {
            if (SDL.SDL_NumJoysticks() <= 0)
            {
                _logger.LogWarning($"There are no Joystick available/connected.");
                return;
            }

            joystickIndex = index;
            JoystickPtr = SDL.SDL_JoystickOpen(joystickIndex);
            if (JoystickPtr == IntPtr.Zero)
            {
                _logger.LogWarning("Manipulator Joystick not found!");
            }
            else
            {
                // Finds the Instance ID and stores it.
                JoystickId = SDL.SDL_JoystickInstanceID(JoystickPtr);
            }
            if (SDL.SDL_JoystickRumble(JoystickPtr, 65535, 65535, 400) == 0)
            {
                Thread.Sleep(500);
                SDL.SDL_JoystickRumble(JoystickPtr, 0, 0, 0);
                Thread.Sleep(500);
                SDL.SDL_JoystickRumble(JoystickPtr, 65535, 65535, 400);
            }
            else
            {
                _logger.LogWarning("Faild to trigger vabration: " + SDL.SDL_GetError());
            }
            _logger.LogInformation("The Manipulator Joystick Has Been Connected.");
        }

        public int GetJoystickId()
        {
            return JoystickId;
        }

        public bool ProcessEvents(SDL.SDL_Event e, CancellationToken cancellationToken)
        {
            // Early exit if cancellation is requested
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Event processing canceled.");
                return false;  // Indicate that event processing was canceled
            }

            // Handle events based on type
            switch (e.type)
            {
                case SDL.SDL_EventType.SDL_JOYBUTTONDOWN:
                case SDL.SDL_EventType.SDL_JOYBUTTONUP:
                    HandleButtonPress(e);
                    break;

                case SDL.SDL_EventType.SDL_JOYAXISMOTION:
                    HandleJoystickMotion(e);
                    break;

                case SDL.SDL_EventType.SDL_JOYHATMOTION:
                    HandleJoyHatMotion(e);
                    break;

                default:
                    return false;  // Event type not handled, return false
            }

            return true;  // Event processed successfully
        }
        // Handle button press events (A, B, X, Y, etc.)
        private void HandleButtonPress(SDL.SDL_Event e)
        {
            // Declare a variable to store the button state (pressed or released)
            int buttonState = 0;

            // Determine the event type and set the buttonState accordingly
            if (e.type == SDL.SDL_EventType.SDL_JOYBUTTONDOWN)
            {
                buttonState = 1; // Button is pressed
            }
            else if (e.type == SDL.SDL_EventType.SDL_JOYBUTTONUP)
            {
                buttonState = 0; // Button is released
            }
            else
            {
                return; // Not a button event, exit early
            }

            // Now handle the button press/release
            mani_buttons[e.jbutton.button] = buttonState;

            switch (e.jbutton.button)
            {
                case 0: _logger.LogDebug($"Button A {(buttonState == 1 ? "pressed" : "released")}"); break;
                case 1: _logger.LogDebug($"Button B {(buttonState == 1 ? "pressed" : "released")}"); break;
                case 2: _logger.LogDebug($"Button X {(buttonState == 1 ? "pressed" : "released")}"); break;
                case 3: _logger.LogDebug($"Button Y {(buttonState == 1 ? "pressed" : "released")}"); break;
                case 4: _logger.LogDebug($"LB {(buttonState == 1 ? "pressed" : "released")}"); break;
                case 5: _logger.LogDebug($"RB {(buttonState == 1 ? "pressed" : "released")}"); break;
                case 8: _logger.LogDebug($"Left Joystick Press {(buttonState == 1 ? "pressed" : "released")}"); break;
                case 9: _logger.LogDebug($"Right Joystick Press {(buttonState == 1 ? "pressed" : "released")}"); break;
                default: _logger.LogDebug($"Unknown button {e.jbutton.button} {(buttonState == 1 ? "pressed" : "released")}"); break;
            }
            mani_buttons[15] = mani_buttons[11] - mani_buttons[12];
        }

        private void HandleJoyHatMotion(SDL.SDL_Event e)
        {
            int x = 0, y = 0;

            switch (e.jhat.hatValue)
            {
                case SDL.SDL_HAT_UP: y = 1; break;
                case SDL.SDL_HAT_DOWN: y = -1; break;
                case SDL.SDL_HAT_LEFT: x = -1; break;
                case SDL.SDL_HAT_RIGHT: x = 1; break;
                case SDL.SDL_HAT_RIGHTUP: x = 1; y = 1; break;
                case SDL.SDL_HAT_RIGHTDOWN: x = 1; y = -1; break;
                case SDL.SDL_HAT_LEFTUP: x = -1; y = 1; break;
                case SDL.SDL_HAT_LEFTDOWN: x = -1; y = -1; break;
                case SDL.SDL_HAT_CENTERED:
                default: x = 0; y = 0; break;
            }

            // Update the stored D-pad state
            mani_dpad = (x, y);

            _logger.LogDebug($"D-pad Position: {mani_dpad}");
        }


        // Handle joystick motion events (Axis motion)
        private void HandleJoystickMotion(SDL.SDL_Event e)
        {
            // Assuming you have already initialized joysticks (JoystickPtr and maniJoystick)
            // Assuming you're working with joystick axes [x, y, z, rotation, 0, 0, 0, 0]
            if (JoystickPtr != IntPtr.Zero)
            {

                int axisIndex = e.jaxis.axis;         // contains the axis index
                short axisValue = e.jaxis.axisValue;  // Get the axis value from the event

                // Normalize and update axis value
                int normalizedValue = NormalizeJoystick(axisIndex, axisValue);
                UpdateAxis(axisIndex, normalizedValue);

                mani_axis[6] = mani_axis[5] - mani_axis[2];

                _logger.LogDebug("Mani Axis: " + string.Join(", ", mani_axis));
            }
        }
        // Update the mani_axis array based on axis index
          private void UpdateAxis(int axisIndex, int value)
        {
            switch (axisIndex)
            {
                case 0: mani_axis[0] = value; break; // X axis
                case 1: mani_axis[1] = value; break; // Y axis
                case 2: mani_axis[2] = value; break; // Z axis
                case 3: mani_axis[3] = value; break; // Rotation
                case 4: mani_axis[4] = value; break; // 
                case 5: mani_axis[5] = value; break; // 

                default: break;
            }
        }

        // Normalize joystick axis values to the range [-100, 100]
        private int NormalizeJoystick(int axisIndex, int value)
        {
            // Normalize the axis value.

            int normalizedValue;

            switch (axisIndex)
            {
                // For axis 1 and 3 (Reversed axes) [-100, 100].
                case 1:
                case 3:
                    normalizedValue = DeadzoneAdjustment((int)(Math.Round((((value - MinValue) / (MaxValue - MinValue) * 2 ) -1) * -100)));
                    break;

                // For axis 2 and 5 (Scaling axes to a range of 0 to 100).
                case 2:
                case 5:
                    normalizedValue = DeadzoneAdjustment((int)(Math.Round(((value - MinValue) / (MaxValue - MinValue)) * 100)));
                    break;

                // Default axis normalization (Handle other axes) [-100, 100].
                default:
                    normalizedValue = DeadzoneAdjustment((int)(Math.Round((((value - MinValue) / (MaxValue - MinValue) * 2 ) -1) * 100)));
                    break;
            }

            return normalizedValue;
        }
        // Function for deadzone adjustment
        private int DeadzoneAdjustment(int value)
        {
            if (Math.Abs(value) < joystickDeadzone+1)
            {
                return 0;
            }
            return value;
        }
        public void CloseJoystick()
        {
            if (JoystickPtr != IntPtr.Zero && SDL.SDL_JoystickGetAttached(JoystickPtr) == SDL.SDL_bool.SDL_TRUE)
            {
                SDL.SDL_JoystickClose(JoystickPtr);
                JoystickPtr = IntPtr.Zero; // Reset pointer to avoid invalid access
                joystickIndex = -1;
                _logger.LogInformation("The Manipulator Joystick Has Been DisConnected.");
            }
        }
                public bool IsRelevantEvent(SDL.SDL_Event e)
        {
            if (e.type == SDL.SDL_EventType.SDL_JOYAXISMOTION ||
                e.type == SDL.SDL_EventType.SDL_JOYBUTTONDOWN ||
                e.type == SDL.SDL_EventType.SDL_JOYBUTTONUP   ||
                e.type == SDL.SDL_EventType.SDL_JOYHATMOTION)
            {
                int eventJoystickId = SDL.SDL_JoystickInstanceID(JoystickPtr);  // Your joystick instance ID
                return eventJoystickId == e.jaxis.which || eventJoystickId == e.jbutton.which || eventJoystickId == e.jhat.which;

            }
            return false;
        }
        public Dictionary<string, object> GetState()
        {
            return new Dictionary<string, object>
            {
                { "mani_axis", (int[])mani_axis.Clone() }, // Prevent accidental modification
                { "mani_buttons", (int[])mani_buttons.Clone() },
                { "mani_dpad", mani_dpad }
            };
        }
    }
}
