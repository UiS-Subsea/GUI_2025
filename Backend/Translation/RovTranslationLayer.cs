
namespace Backend.Translation
{
    public class RovTranslationLayer
    {
        public List<object> Translate(Dictionary<string, object> rovState)
        {
            var commands = new List<object>();

            // Check for each possible input and call the correct method
            if (rovState.TryGetValue("rov_axis", out var axisObj) && axisObj is int[] rov_axis)
                commands.Add(BuildRovPacket(rov_axis));

            if (rovState.TryGetValue("mani_dpad", out var maniDpadObj) && maniDpadObj is int[] mani_dpad
                && rovState.TryGetValue("mani_joystick", out var maniJoystickObj) && maniJoystickObj is int[] mani_joystick)
                commands.Add(BuildManiPacket(mani_dpad, mani_joystick));

            if (rovState.TryGetValue("autonom_data", out var autoObj) && autoObj is int[] autonom_data)
                commands.Add(BuildAutonomPacket(autonom_data));

            if (rovState.TryGetValue("Controls_Reset", out var controlsObj) && controlsObj is int[] controlsReset)
                commands.Add(BuildControlsResetPacket(controlsReset));

            if (rovState.TryGetValue("Thruster_Controls_Reset", out var thrusterObj) && thrusterObj is int[] thrusterReset)
                commands.Add(BuildThrusterControlsResetPacket(thrusterReset));

            if (rovState.TryGetValue("Manipulator_Controls_Reset", out var manipulatorObj) && manipulatorObj is int[] manipulatorReset)
                commands.Add(BuildManipulatorControlsResetPacket(manipulatorReset));

            if (rovState.TryGetValue("Depth_Reset", out var depthObj) && depthObj is int[] depthReset)
                commands.Add(BuildDepthResetPacket(depthReset));

            if (rovState.TryGetValue("Angles_Reset", out var anglesObj) && anglesObj is int[] anglesReset)
                commands.Add(BuildAnglesResetPacket(anglesReset));

            if (rovState.TryGetValue("IMU_Calibrate", out var imuObj) && imuObj is int[] imuCalibrate)
                commands.Add(BuildIMUCalibratePacket(imuCalibrate));

            if (rovState.TryGetValue("Regulator_Tuning", out var regulatorObj) && regulatorObj is int[] regulatorTuning)
                commands.Add(BuildRegulatorTuningPacket(regulatorTuning));

            if (rovState.TryGetValue("Toggle_All_Regulator", out var toggleAllObj) && toggleAllObj is int[] toggleAll)
                commands.Add(BuildToggleAllRegulatorPacket(toggleAll));

            if (rovState.TryGetValue("Toggle_Roll_Regulator", out var toggleRollObj) && toggleRollObj is int[] toggleRoll)
                commands.Add(BuildToggleRollRegulatorPacket(toggleRoll));

            if (rovState.TryGetValue("Toggle_Stamp_Regulator", out var toggleStampObj) && toggleStampObj is int[] toggleStamp)
                commands.Add(BuildToggleStampRegulatorPacket(toggleStamp));

            if (rovState.TryGetValue("Toggle_Depth_Regulator", out var toggleDepthObj) && toggleDepthObj is int[] toggleDepth)
                commands.Add(BuildToggleDepthRegulatorPacket(toggleDepth));

            if (rovState.TryGetValue("Front_Light_On", out var frontLightObj) && frontLightObj is int[] frontLight)
                commands.Add(BuildFrontLightPacket(frontLight));

            if (rovState.TryGetValue("Bottom_Light_On", out var bottomLightObj) && bottomLightObj is int[] bottomLight)
                commands.Add(BuildBottomLightPacket(bottomLight));

            if (rovState.TryGetValue("Front_Light_Slider", out var frontLightSliderObj) && frontLightSliderObj is int[] frontLightSlider)
                commands.Add(BuildFrontLightIntensityPacket(frontLightSlider));

            if (rovState.TryGetValue("Bottom_Light_Slider", out var bottomLightSliderObj) && bottomLightSliderObj is int[] bottomLightSlider)
                commands.Add(BuildBottomLightIntensityPacket(bottomLightSlider));

            if (rovState.TryGetValue("tilt", out var tiltObj) && tiltObj is int[] tiltData)
                commands.Add(BuildCameraTiltPacket(tiltData));

            return commands;
        }


        private object BuildRovPacket(int[] rov_axis)
        {
            return new object[]
            {
                33, new int[]
                {
                    GetValue(rov_axis, 1), // X axis
                    GetValue(rov_axis, 0), // Y axis
                    GetValue(rov_axis, 6), // Z axis
                    GetValue(rov_axis, 3), // Rotation
                    0, 0, 0, 0
                }
            };
        }

        private object BuildAutonomPacket(int[] autonomdata)
        {
            return new object[]
            {
                33, new int[]
                {
                    GetValue(autonomdata, 0), // X axis
                    GetValue(autonomdata, 1), // Y axis
                    GetValue(autonomdata, 2), // Z axis
                    GetValue(autonomdata, 3), // Rotation
                    0, 0, 0, 0
                }
            };
        }

        private object BuildManiPacket(int[] mani_dpad, int[] mani_joystick)
        {
            return new object[]
            {
                34, new int[]
                {
                    GetValue(mani_dpad, 1) *100,
                    GetValue(mani_joystick, 0), //MANIPULATOR ROTATION
                    GetValue(mani_joystick, 4), //MANIPULATOR TILT
                    GetValue(mani_joystick, 6), //MANIPULATOR GRAB RELEASE
                    0, 0, 0, 0,
                }
            };
        }

        private object BuildControlsResetPacket(int[] Controls_Reset)
        {
            return new object[]
            {
                97, new int[]
                {
                    GetValue(Controls_Reset, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            };
        }

        private object BuildThrusterControlsResetPacket(int[] Thruster_Controls_Reset)
        {
            return new object[]
            {
                98, new int[]
                {
                    GetValue(Thruster_Controls_Reset, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            };
        }

        private object BuildManipulatorControlsResetPacket(int[] Manipulator_Controls_Reset)
        {
            return new object[]
            {
                99, new int[]
                {
                    GetValue(Manipulator_Controls_Reset, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            };
        }

        private object BuildDepthResetPacket(int[] Depth_Reset)
        {
            return new object[]
            {
                66, new int[]
                {
                    GetValue(Depth_Reset, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            };
        }

        private object BuildAnglesResetPacket(int[] Angles_Reset)
        {
            return new object[]
            {
                66, new int[]
                {
                    GetValue(Angles_Reset, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            };
        }

        private object BuildIMUCalibratePacket(int[] IMU_Calibrate)
        {
            return new object[]
            {
                66, new int[]
                {
                    GetValue(IMU_Calibrate, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            };
        }

        private object BuildRegulatorTuningPacket(int[] Regulator_Tuning)
        {
            return new object[]
            {
                42, new int[]
                {
                    GetValue(Regulator_Tuning, 0),
                    GetValue(Regulator_Tuning, 1),
                    0, 0, 0, 0, 0, 0,
                }
            };
        }

        private object BuildToggleAllRegulatorPacket(int[] Toggle_All_Regulator)
        {
            return new object[]
            {
                32, new int[]
                {
                    GetValue(Toggle_All_Regulator, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            };
        }

        private object BuildToggleRollRegulatorPacket(int[] Toggle_Roll_Regulator)
        {
            return new object[]
            {
                32, new int[]
                {
                    GetValue(Toggle_Roll_Regulator, 0), //rull i gammle gui
                    0, 0, 0, 0, 0, 0, 0,
                }
            };
        }

        private object BuildToggleStampRegulatorPacket(int[] Toggle_Stamp_Regulator)
        {
            return new object[]
            {
                32, new int[]
                {
                    GetValue(Toggle_Stamp_Regulator, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            };
        }

        private object BuildToggleDepthRegulatorPacket(int[] Toggle_Depth_Regulator)
        {
            return new object[]
            {
                32, new int[]
                {
                    GetValue(Toggle_Depth_Regulator, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            };
        }

        private object BuildFrontLightPacket(int[] Front_Light_On)
        {
            return new object[]
            {
                98, new int[]
                {
                    GetValue(Front_Light_On, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            };
        }

        private object BuildBottomLightPacket(int[] Bottom_Light_On)
        {
            return new object[]
            {
                99, new int[]
                {
                    GetValue(Bottom_Light_On, 0),
                    0, 0, 0, 0, 0, 0, 0,
                }
            };
        }

        private object BuildFrontLightIntensityPacket(int[] Front_Light_Slider)
        {
            return new object[]
            {
                98, new int[]
                {
                    0,
                    GetValue(Front_Light_Slider, 1),
                    0, 0, 0, 0, 0, 0,
                }
            };
        }

        private object BuildBottomLightIntensityPacket(int[] Bottom_Light_Slider)
        {
            return new object[]
            {
                99, new int[]
                {
                    0,
                    GetValue(Bottom_Light_Slider, 1),
                    0, 0, 0, 0, 0, 0,
                }
            };
        }

        private object BuildCameraTiltPacket(int[] tilt)
        {
            return new object[]
            {
                200, new object[] { "tilt", GetValue(tilt, 0) }
            };
        }

        private int GetValue(int[] array, int index)
        {
            return index < array.Length ? array[index] : 0;
        }
    }
}
