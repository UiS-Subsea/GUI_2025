
using System.Net.Sockets;
using Backend.Infrastructure.Interface;

namespace Backend.Translation
{
    public class RovTranslationLayer : IRovTranslationLayer
    {
        public List<object> Translate(Dictionary<string, object> rovCommand)
        {
            var commands = new List<object>();

            // Check for each possible input and call the correct method
            if (rovCommand.TryGetValue("rov_axis", out var axisObj) && axisObj is int[] rov_axis)
                commands.Add(BuildRovPacket(rov_axis));

            if (rovCommand.TryGetValue("mani_dpad", out var maniDpadObj) && maniDpadObj is int[] mani_dpad
                && rovCommand.TryGetValue("mani_joystick", out var maniJoystickObj) && maniJoystickObj is int[] mani_joystick)
                commands.Add(BuildManiPacket(mani_dpad, mani_joystick));

            if (rovCommand.TryGetValue("autonom_data", out var autoObj) && autoObj is int[] autonom_data)
                commands.Add(BuildAutonomPacket(autonom_data));

            if (rovCommand.TryGetValue("Controls_Reset", out var controlsObj) && controlsObj is int[] controlsReset)
                commands.Add(BuildControlsResetPacket(controlsReset));

            if (rovCommand.TryGetValue("Thruster_Controls_Reset", out var thrusterObj) && thrusterObj is int[] thrusterReset)
                commands.Add(BuildThrusterControlsResetPacket(thrusterReset));

            if (rovCommand.TryGetValue("Manipulator_Controls_Reset", out var manipulatorObj) && manipulatorObj is int[] manipulatorReset)
                commands.Add(BuildManipulatorControlsResetPacket(manipulatorReset));

            if (rovCommand.TryGetValue("Depth_Reset", out var depthObj) && depthObj is int[] depthReset)
                commands.Add(BuildDepthResetPacket(depthReset));

            if (rovCommand.TryGetValue("Angles_Reset", out var anglesObj) && anglesObj is int[] anglesReset)
                commands.Add(BuildAnglesResetPacket(anglesReset));

            if (rovCommand.TryGetValue("IMU_Calibrate", out var imuObj) && imuObj is int[] imuCalibrate)
                commands.Add(BuildIMUCalibratePacket(imuCalibrate));

            if (rovCommand.TryGetValue("Regulator_Tuning", out var regulatorObj) && regulatorObj is int[] regulatorTuning)
                commands.Add(BuildRegulatorTuningPacket(regulatorTuning));

            if (rovCommand.TryGetValue("Toggle_All_Regulator", out var toggleAllObj) && toggleAllObj is int[] toggleAll)
                commands.Add(BuildToggleAllRegulatorPacket(toggleAll));

            if (rovCommand.TryGetValue("Toggle_Roll_Regulator", out var toggleRollObj) && toggleRollObj is int[] toggleRoll)
                commands.Add(BuildToggleRollRegulatorPacket(toggleRoll));

            if (rovCommand.TryGetValue("Toggle_Stamp_Regulator", out var toggleStampObj) && toggleStampObj is int[] toggleStamp)
                commands.Add(BuildToggleStampRegulatorPacket(toggleStamp));

            if (rovCommand.TryGetValue("Toggle_Depth_Regulator", out var toggleDepthObj) && toggleDepthObj is int[] toggleDepth)
                commands.Add(BuildToggleDepthRegulatorPacket(toggleDepth));

            if (rovCommand.TryGetValue("Front_Light_On", out var frontLightObj) && frontLightObj is int[] frontLight)
                commands.Add(BuildFrontLightPacket(frontLight));

            if (rovCommand.TryGetValue("Bottom_Light_On", out var bottomLightObj) && bottomLightObj is int[] bottomLight)
                commands.Add(BuildBottomLightPacket(bottomLight));

            if (rovCommand.TryGetValue("Front_Light_Slider", out var frontLightSliderObj) && frontLightSliderObj is int[] frontLightSlider)
                commands.Add(BuildFrontLightIntensityPacket(frontLightSlider));

            if (rovCommand.TryGetValue("Bottom_Light_Slider", out var bottomLightSliderObj) && bottomLightSliderObj is int[] bottomLightSlider)
                commands.Add(BuildBottomLightIntensityPacket(bottomLightSlider));

            if (rovCommand.TryGetValue("tilt", out var tiltObj) && tiltObj is int[] tiltData)
                commands.Add(BuildCameraTiltPacket(tiltData));

            return commands;
        }


        private Object[] BuildRovPacket(int[] rov_axis)
        {
            return
            [
                33,
                new int[]
                {
                    GetValue(rov_axis, 1), // X axis
                    GetValue(rov_axis, 0), // Y axis
                    GetValue(rov_axis, 6), // Z axis
                    GetValue(rov_axis, 3), // Rotation
                    0, 0, 0, 0
                }
            ];
        }

        private Object[] BuildAutonomPacket(int[] autonomdata)
        {
            return
            [ 
                33,
                new int[]
                {
                    GetValue(autonomdata, 0), // X axis
                    GetValue(autonomdata, 1), // Y axis
                    GetValue(autonomdata, 2), // Z axis
                    GetValue(autonomdata, 3), // Rotation
                    0, 0, 0, 0
                }
            ];
        }

        private Object[] BuildManiPacket(int[] mani_dpad, int[] mani_joystick)
        {
            return 
            [
                34,
                new int[]
                {
                    GetValue(mani_dpad, 1) *100,
                    GetValue(mani_joystick, 0), //MANIPULATOR ROTATION
                    GetValue(mani_joystick, 4), //MANIPULATOR TILT
                    GetValue(mani_joystick, 6), //MANIPULATOR GRAB RELEASE
                    0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildControlsResetPacket(int[] Controls_Reset)
        {
            return 
            [
                97,
                new int[]
                {
                    GetValue(Controls_Reset, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildThrusterControlsResetPacket(int[] Thruster_Controls_Reset)
        {
            return 
            [
                98,
                new int[]
                {
                    GetValue(Thruster_Controls_Reset, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildManipulatorControlsResetPacket(int[] Manipulator_Controls_Reset)
        {
            return 
            [
                99,
                new int[]
                {
                    GetValue(Manipulator_Controls_Reset, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildDepthResetPacket(int[] Depth_Reset)
        {
            return
            [
                66,
                new int[]
                {
                    GetValue(Depth_Reset, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildAnglesResetPacket(int[] Angles_Reset)
        {
            return
            [
                66,
                new int[]
                {
                    GetValue(Angles_Reset, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildIMUCalibratePacket(int[] IMU_Calibrate)
        {
            return 
            [
                66,
                new int[]
                {
                    GetValue(IMU_Calibrate, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildRegulatorTuningPacket(int[] Regulator_Tuning)
        {
            return 
            [
                42,
                new int[]
                {
                    GetValue(Regulator_Tuning, 0),
                    GetValue(Regulator_Tuning, 1),
                    0, 0, 0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildToggleAllRegulatorPacket(int[] Toggle_All_Regulator)
        {
            return
            [
                32,
                new int[]
                {
                    GetValue(Toggle_All_Regulator, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildToggleRollRegulatorPacket(int[] Toggle_Roll_Regulator)
        {
            return
            [
                32,
                new int[]
                {
                    GetValue(Toggle_Roll_Regulator, 0), //rull i gammle gui
                    0, 0, 0, 0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildToggleStampRegulatorPacket(int[] Toggle_Stamp_Regulator)
        {
            return
            [
                32,
                new int[]
                {
                    GetValue(Toggle_Stamp_Regulator, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildToggleDepthRegulatorPacket(int[] Toggle_Depth_Regulator)
        {
            return
            [
                32,
                new int[]
                {
                    GetValue(Toggle_Depth_Regulator, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildFrontLightPacket(int[] Front_Light_On)
        {
            return
            [
                98, 
                new int[]
                {
                    GetValue(Front_Light_On, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildBottomLightPacket(int[] Bottom_Light_On)
        {
            return
            [
                99, 
                new int[]
                {
                    GetValue(Bottom_Light_On, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildFrontLightIntensityPacket(int[] Front_Light_Slider)
        {
            return
            [
                98, 
                new int[]
                {
                    0,
                    GetValue(Front_Light_Slider, 1),
                    0, 0, 0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildBottomLightIntensityPacket(int[] Bottom_Light_Slider)
        {
            return
            [
                99, 
                new int[]
                {
                    0,
                    GetValue(Bottom_Light_Slider, 1),
                    0, 0, 0, 0, 0, 0,
                }
            ];
        }

        private Object[] BuildCameraTiltPacket(int[] tilt)
        {
            return
            [
                200,
                new object[]
                {
                     "tilt", GetValue(tilt, 0)
                }
            ];
        }
        /*
        private Object[] BuildRegModePacket(int[] tilt)
        {
            return
            [
                32,
                new int[]
                {
                    GetValue(RegMode, 1),
                    0, 0, 0, 0, 0, 0, 0,
                }
            ];
        }
        private Object[] BuildPIDParametersPacket(int[] tilt)
        {
            return
            [
                42,
                new int[]
                {
                    GetValue(PIDParameters, 0), // Degree of Freedom
                    GetValue(PIDParameters, 1), // KP
                    GetValue(PIDParameters, 2), // KI
                    GetValue(PIDParameters, 3), // KD
                }
            ];
        }
        private Object[] BuildStartAutotunePacket(int[] tilt)
        {
            return
            [
                43,
                new int[]
                {
                    GetValue(rov_axis, 0), // Start  0= NO, 1 = yes
                    GetValue(rov_axis, 1), // Abort  0= NO, 1 = yes
                    GetValue(rov_axis, 2), // Degree of Freedom
                    GetValue(rov_axis, 3), // Stepsize
                    0, 0, 0, 0,
                }
            ];
        }
        private Object[] BuildRegModeSettingsPacket(int[] tilt)
        {
            return
            [
                300,
                new float[]
                {
                    GetValue(rov_axis, 0), // Reference X (WPT) , Float
                    GetValue(rov_axis, 1), // Reference Y (WPT) , Float
                    GetValue(rov_axis, 2), // Reference Z (WPT) , Float
                    GetValue(rov_axis, 3), // Reference PSI (WPT) , Float
                    GetValue(rov_axis, 4), // Chosen Trajectory  , Int
                    GetValue(rov_axis, 5), // Chosen Speed  , Float
                }
            ];
        }
        private Object[] BuildRegMPCSettingsPacket(int[] tilt)
        {
            return
            [
                301,
                new float[]
                {
                    GetValue(rov_axis, 0), // Qx
                    GetValue(rov_axis, 1), // Qy
                    GetValue(rov_axis, 2), // Qz
                    GetValue(rov_axis, 3), // Qpsi
                    GetValue(rov_axis, 4), // Ru
                    GetValue(rov_axis, 5), // Rv
                    GetValue(rov_axis, 6), // Rw
                    GetValue(rov_axis, 7), // Rr
                    GetValue(rov_axis, 8), // Dt
                    GetValue(rov_axis, 9), // N
                    GetValue(rov_axis, 10), // vel_u_max
                    GetValue(rov_axis, 11), // vel_v_max
                    GetValue(rov_axis, 12), // vel_w_max
                    GetValue(rov_axis, 13), // vel_r_max
                    GetValue(rov_axis, 14), // acc_u_max
                    GetValue(rov_axis, 15), // acc_v_max
                    GetValue(rov_axis, 16), // acc_w_max
                    GetValue(rov_axis, 17), // acc_r_max
                }
            ];
        }
        */
        private int GetValue(int[] array, int index)
        {
            return index < array.Length ? array[index] : 0;
        }
    }
}
