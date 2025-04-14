
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

            if (rovCommand.TryGetValue("reg_mode", out var regModeObj) && regModeObj is int[] reg_mode)
                commands.Add(BuildRegModePacket(reg_mode));
            
            if (rovCommand.TryGetValue("pid_settings", out var pidParamsObj) && pidParamsObj is int[] pid_settings)
                commands.Add(BuildPIDParametersPacket(pid_settings));

            if (rovCommand.TryGetValue("autotune", out var autotuneObj) && autotuneObj is int[] autotune)
                commands.Add(BuildStartAutotunePacket(autotune));

            if (rovCommand.TryGetValue("reg_mode_setting", out var regModeSettingsObj) && regModeSettingsObj is float[] reg_mode_setting)
                commands.Add(BuildRegModeSettingsPacket(reg_mode_setting));

            if (rovCommand.TryGetValue("mpc_settings", out var mpcSettingsObj) && mpcSettingsObj is float[] mpc_settings)
                commands.Add(BuildRegMPCSettingsPacket(mpc_settings));

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
        
        private Object[] BuildRegModePacket(int[] reg_mode)
        {
            return
            [
                32,
                new int[]
                {
                    GetValue(reg_mode, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            ];
        }
        private Object[] BuildPIDParametersPacket(int[] pid_settings)
        {
            return
            [
                42,
                new int[]
                {
                    GetValue(pid_settings, 0), // Degree of Freedom
                    GetValue(pid_settings, 1), // KP
                    GetValue(pid_settings, 2), // KI
                    GetValue(pid_settings, 3), // KD
                }
            ];
        }
        private Object[] BuildStartAutotunePacket(int[] autotune)
        {
            return
            [
                43,
                new int[]
                {
                    GetValue(autotune, 0), // Start  0= NO, 1 = yes
                    GetValue(autotune, 1), // Abort  0= NO, 1 = yes
                    GetValue(autotune, 2), // Degree of Freedom
                    GetValue(autotune, 3), // Stepsize
                    0, 0, 0, 0,
                }
            ];
        }
        private Object[] BuildRegModeSettingsPacket(float[] reg_mode_setting)
        {
            return
            [
                300,
                new float[]
                {
                    GetValue(reg_mode_setting, 0), // Reference X (WPT) , Float
                    GetValue(reg_mode_setting, 1), // Reference Y (WPT) , Float
                    GetValue(reg_mode_setting, 2), // Reference Z (WPT) , Float
                    GetValue(reg_mode_setting, 3), // Reference PSI (WPT) , Float
                    GetValue(reg_mode_setting, 4), // Chosen Trajectory  , Int
                    GetValue(reg_mode_setting, 5), // Chosen Speed  , Float
                }
            ];
        }
        private Object[] BuildRegMPCSettingsPacket(float[] mpc_settings)
        {
            return
            [
                301,
                new float[]
                {
                    GetValue(mpc_settings, 0), // Qx
                    GetValue(mpc_settings, 1), // Qy
                    GetValue(mpc_settings, 2), // Qz
                    GetValue(mpc_settings, 3), // Qpsi
                    GetValue(mpc_settings, 4), // Ru
                    GetValue(mpc_settings, 5), // Rv
                    GetValue(mpc_settings, 6), // Rw
                    GetValue(mpc_settings, 7), // Rr
                    GetValue(mpc_settings, 8), // Dt
                    GetValue(mpc_settings, 9), // N
                    GetValue(mpc_settings, 10), // vel_u_max
                    GetValue(mpc_settings, 11), // vel_v_max
                    GetValue(mpc_settings, 12), // vel_w_max
                    GetValue(mpc_settings, 13), // vel_r_max
                    GetValue(mpc_settings, 14), // acc_u_max
                    GetValue(mpc_settings, 15), // acc_v_max
                    GetValue(mpc_settings, 16), // acc_w_max
                    GetValue(mpc_settings, 17), // acc_r_max
                }
            ];
        }
        
        private int GetValue(int[] array, int index)
        {
            return index < array.Length ? array[index] : 0;
        }
        private float GetValue(float[] array, int index)
        {
            return index < array.Length ? array[index] : 0f;
        }
    }
}
