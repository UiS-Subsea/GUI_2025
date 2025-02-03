using SDL2;

namespace Backend.Domain.ROV_Controller
{
    public class RovController
    {

        private IntPtr JoystickPtr; 
        private int joystickIndex = 0; // The First Joystick Becomes Rov-Controller.
        private Guid joystickGUID; // Store The Xbox One Controllers GUID, For reconnecting The Same Controller.
        public bool IsConnected => JoystickPtr != IntPtr.Zero && SDL.SDL_JoystickGetAttached(JoystickPtr) == SDL.SDL_bool.SDL_TRUE;

        private const int joystickDeadzone = 15; // The deadzone Value.
        private const float MaxValue = 32767.0f; // The Xbox One Controllers Max Joystick Value.
        private const float MinValue = -32768.0f; // The Xbox One Controllers Min Joystick Value.

        private int[] rov_buttons = new int[15]; // Store button states (0 or 1).
        private float[] rov_axis = new float[8]; // Store joystick axis states in [x, y, z, rotation, 0, 0, 0, 0].
        private (int x, int y) rov_dpad = (0, 0);

        public RovController() {}

        public void InitializeJoystick()
        {
            if (SDL.SDL_NumJoysticks() <= 0)
            {
                Console.WriteLine($"There are no Joystick available/connected.");
                return;
            }
            JoystickPtr = SDL.SDL_JoystickOpen(joystickIndex);
            if (JoystickPtr == IntPtr.Zero)
            {
                Console.WriteLine("Joystick not found!");
            }
            else
            {
                // Convert SDL_JoystickGUID to a System.Guid
                joystickGUID = SDL.SDL_JoystickGetDeviceGUID(joystickIndex);
                Console.WriteLine($"Joystick {joystickIndex} connected. (GUID: {joystickGUID})");
            }
            Console.WriteLine("There are: " + SDL.SDL_NumJoysticks() + "  Controllers");
        }

        public bool CheckJoystickConnection()
        {
            // Check if joystick is still attached and matches the expected instance ID
            if (!IsConnected)
            {
                Console.WriteLine("Joystick disconnected. Reconnecting...");
                for (int i = 0; i < SDL.SDL_NumJoysticks(); i++)
                {
                    Guid newGUID = SDL.SDL_JoystickGetDeviceGUID(i);
                    if (joystickGUID == newGUID)
                    {
                        joystickIndex = i;
                        InitializeJoystick();
                        Console.WriteLine($"Reconnected joystick with matching GUID at index {i}");
                        return true;
                    }
                }
                Console.WriteLine("No matching joystick found. Waiting for reconnection...");
                return false;
            }
            return true;
        }

        public Dictionary<string, object> ProcessEvents(SDL.SDL_Event e, CancellationToken cancellationToken)
        {
            if (e.type == SDL.SDL_EventType.SDL_JOYBUTTONDOWN || e.type == SDL.SDL_EventType.SDL_JOYBUTTONUP)
            {
                HandleButtonPress(e);
            }

            if (e.type == SDL.SDL_EventType.SDL_JOYAXISMOTION)
            {
                HandleJoystickMotion(e);
            }
            if (e.type == SDL.SDL_EventType.SDL_JOYHATMOTION)
            {
                HandleJoyHatMotion(e);
            }
            return new Dictionary<string, object>
            {
                { "rov_axis", rov_axis },
                { "rov_buttons", rov_buttons }
            };
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

            // Now handle the button press/release using a single switch statement
            switch (e.jbutton.button)
            {
                case 0: // "A" button
                    rov_buttons[0] = buttonState;
                    Console.WriteLine($"Button A {(buttonState == 1 ? "pressed" : "released")}");
                    break;
                case 1: // "B" button
                    rov_buttons[1] = buttonState;
                    Console.WriteLine($"Button B {(buttonState == 1 ? "pressed" : "released")}");
                    break;
                case 2: // "X" button
                    rov_buttons[2] = buttonState;
                    Console.WriteLine($"Button X {(buttonState == 1 ? "pressed" : "released")}");
                    break;
                case 3: // "Y" button
                    rov_buttons[3] = buttonState;
                    Console.WriteLine($"Button Y {(buttonState == 1 ? "pressed" : "released")}");
                    break;
                case 7: // "Left Joystick Press"
                    rov_buttons[7] = buttonState;
                    Console.WriteLine($"Left Joystick Press {(buttonState == 1 ? "pressed" : "released")}");
                    break;
                case 8: // "Right Joystick Press"
                    rov_buttons[8] = buttonState;
                    Console.WriteLine($"Right Joystick Press {(buttonState == 1 ? "pressed" : "released")}");
                    break;
                case 9: // "LB" (Left Bumper)
                    rov_buttons[9] = buttonState;
                    Console.WriteLine($"LB {(buttonState == 1 ? "pressed" : "released")}");
                    break;
                case 10: // "RB" (Right Bumper)
                    rov_buttons[10] = buttonState;
                    Console.WriteLine($"RB {(buttonState == 1 ? "pressed" : "released")}");
                    break;
                case 11: // "DPAD UP"
                    rov_buttons[11] = buttonState;
                    Console.WriteLine($"DPAD UP {(buttonState == 1 ? "pressed" : "released")}");
                    break;
                case 12: // "DPAD DOWN"
                    rov_buttons[12] = buttonState;
                    Console.WriteLine($"DPAD DOWN {(buttonState == 1 ? "pressed" : "released")}");
                    break;
                case 13: // "DPAD LEFT"
                    rov_buttons[13] = buttonState;
                    Console.WriteLine($"DPAD LEFT {(buttonState == 1 ? "pressed" : "released")}");
                    break;
                case 14: // "DPAD RIGHT"
                    rov_buttons[14] = buttonState;
                    Console.WriteLine($"DPAD RIGHT {(buttonState == 1 ? "pressed" : "released")}");
                    break;
                default:
                    Console.WriteLine($"Unknown button {e.jbutton.button} {(buttonState == 1 ? "pressed" : "released")}");
                    break;
            }
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
            rov_dpad = (x, y);

            Console.WriteLine($"D-pad Position: {rov_dpad}");
        }


        // Handle joystick motion events (Axis motion)
        private void HandleJoystickMotion(SDL.SDL_Event e)
        {
            // Assuming you have already initialized joysticks (JoystickPtr and maniJoystick)
            // Assuming you're working with joystick axes [x, y, z, rotation, 0, 0, 0, 0]
            if (JoystickPtr != IntPtr.Zero)
            {
                for (int i = 0; i < SDL.SDL_JoystickNumAxes(JoystickPtr); i++)
                {
                    int normalizedValue = NormalizeJoystick(JoystickPtr, i);
                    UpdateAxis(i, normalizedValue);
                }

                Console.WriteLine("ROV Axis: " + string.Join(", ", rov_axis));

            }
        }
        // Update the rov_axis array based on axis index
          private void UpdateAxis(int axisIndex, int value)
        {
            switch (axisIndex)
            {
                case 0: rov_axis[0] = value; break; // X axis
                case 1: rov_axis[1] = value; break; // Y axis
                case 2: rov_axis[2] = value; break; // Z axis
                case 3: rov_axis[3] = value; break; // Rotation
                case 4: rov_axis[4] = value; break; // 
                case 5: rov_axis[5] = value; break; // 

                default: break;
            }
        }

        // Normalize joystick axis values to the range [-100, 100]
        private int NormalizeJoystick(IntPtr joystick, int axis)
        {
            // Normalize the axis value.
            
            short axisValue = SDL.SDL_JoystickGetAxis(joystick, axis); // Get the axis value from SDL2.

            int normalizedValue;

            switch (axis)
            {
                // For axis 1 and 3 (Reversed axes) [-100, 100].
                case 1:
                case 3:
                    normalizedValue = DeadzoneAdjustment((int)(Math.Round((((axisValue - MinValue) / (MaxValue - MinValue) * 2 )-1)*-100)));
                    break;

                // For axis 2 and 5 (Scaling axes to a range of 0 to 100).
                case 2:
                case 5:
                    normalizedValue = DeadzoneAdjustment((int)(Math.Round(((axisValue - MinValue) / (MaxValue - MinValue)) * 100)));
                    break;

                // Default axis normalization (Handle other axes) [-100, 100].
                default:
                    normalizedValue = DeadzoneAdjustment((int)(Math.Round(((axisValue - MinValue) / (MaxValue - MinValue) *2) -1) * 100));
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
            }
        }
        public bool IsRelevantEvent(SDL.SDL_Event e)
        {
            if (e.type == SDL.SDL_EventType.SDL_JOYAXISMOTION ||
                e.type == SDL.SDL_EventType.SDL_JOYBUTTONDOWN ||
                e.type == SDL.SDL_EventType.SDL_JOYBUTTONUP)
            {
                int eventJoystickId = SDL.SDL_JoystickInstanceID(JoystickPtr);  // Your joystick instance ID
                return eventJoystickId == e.jaxis.which || eventJoystickId == e.jbutton.which;
            }
            return false;
        }
    }
}
