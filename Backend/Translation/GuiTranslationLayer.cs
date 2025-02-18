using System.Text;
using System.Text.Json;

namespace Backend.Translation
{  
    public class GUITranslationLayer
    {
        private string _incompletePacket = ""; // Store unfinished packets

        public List<object> DecodeAndTranslatePackets(byte[] tcpData)
        {
            List<object> translatedPackets = new();

            try
            {
                string dataStr = _incompletePacket + Encoding.UTF8.GetString(tcpData);
                _incompletePacket = "";

                if (!dataStr.StartsWith("*")) return translatedPackets;
                if (!dataStr.EndsWith("*"))
                {
                    int lastStarIndex = dataStr.LastIndexOf("*");
                    _incompletePacket = dataStr.Substring(lastStarIndex);
                    dataStr = dataStr.Substring(0, lastStarIndex);
                }

                string[] jsonStrings = dataStr.Split('*', StringSplitOptions.RemoveEmptyEntries);

                foreach (string packet in jsonStrings)
                {
                    try
                    {
                        var parsedJson = JsonSerializer.Deserialize<List<JsonElement>>(packet);
                        if (parsedJson == null || parsedJson.Count < 2) continue;

                        int canId = parsedJson[0].GetInt32(); // Convert first element to int
                        var rawData = parsedJson[1];
                        List<object> convertedData = ConvertJsonData(rawData);


                        var translated = TranslateData(new List<object> { canId, convertedData });
                        if (translated != null)
                            translatedPackets.Add(translated);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON Parsing Error: {ex.Message}, Packet: {packet}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decoding TCP data: {ex.Message}");
            }

            return translatedPackets;
        }

        // Translates data differently based on CAN ID
        private object? TranslateData(List<object> data)
        {

            if (data.Count < 2 || data[0] is not int canId || data[1] is not List<object> dataArray)
                return null;

            List<object> values = dataArray;

            // Different mapping based on CAN ID
            return canId switch
            {
                129 => new { Type = "THRUSTPAADRAG",
                                HFF = Math.Round(Convert.ToDouble(values[0]), 2), 
                                HHB = Math.Round(Convert.ToDouble(values[1]), 2), 
                                HVB = Math.Round(Convert.ToDouble(values[2]), 2), 
                                HVF = Math.Round(Convert.ToDouble(values[3]), 2), 
                                VHF = Math.Round(Convert.ToDouble(values[4]), 2), 
                                VHB = Math.Round(Convert.ToDouble(values[5]), 2), 
                                VVB = Math.Round(Convert.ToDouble(values[6]), 2), 
                                VVF = Math.Round(Convert.ToDouble(values[7]), 2) },

                130 => new { Type = "REGTEMP", 
                                REG_temp = Math.Round(Convert.ToDouble(values[0])/100, 2),
                                Motor_temp = Math.Round(Convert.ToDouble(values[1])/100,2),
                                Depth = Math.Round(Convert.ToDouble(values[2])/100, 2) },

                135 => new { Type = "AKSELERASJON", Roll = values[0], Pitch = values[1], Yaw = values[2] },// doesn't look like this is used in GUI

                136 => new { Type = "GYRO", Roll = values[0], Pitch = values[1], Yaw = values[2] },// doesn't look like this is used in GUI

                137 => new { Type = "MAGNETOMETER", Roll = values[0], Pitch = values[1], Yaw = values[2] },// doesn't look like this is used in GUI

                138 => new { Type = "VINKLER", 
                                Roll = Math.Round(Convert.ToDouble(values[0]), 2),            //*
                                Stamp = Math.Round(Convert.ToDouble(values[1]), 2),           //*
                                Gir = Math.Round(Convert.ToDouble(values[2]), 2) },           //*

                139 => new { Type = "TEMPDYBDE", 
                                Depth = Math.Round(Convert.ToDouble(values[0]), 2),           //cm
                                Water_temp = Math.Round(Convert.ToDouble(values[1]), 2),      //*C
                                Sensor_temp = Math.Round(Convert.ToDouble(values[2])/100, 2) },   //*C

                140 => new { Type = "SENSORERROR", 
                                IMU_Errors = SetErrors((List<bool>)values[0], 0),                   //List of Error strings
                                TEMP_Errors = SetErrors((List<bool>)values[1], 1),                  //List of Error strings
                                PRESSURE_Errors = SetErrors((List<bool>)values[2], 2),              //List of Error strings
                                Leak_Errors = SetErrors((List<bool>)values[3], 3) },                //List of Error strings

                // CommunicationController Cards Temperature.
                145 => new { Type = "COMTEMP", 
                                Com_temp = Math.Round(Convert.ToDouble(values[0]), 2) },      //*C

                150 => new { Type = "DATA12VRIGHT", 
                                Power = Math.Round(Convert.ToDouble(values[0])/1000, 2),                  //A
                                Temp = Math.Round(Convert.ToDouble(values[1])/100, 2),                    //*C
                                Fuse = SetPowerErrors((List<bool>)values[2]) },                     //PowerError Strings

                151 => new { Type = "DATA12VLEFT", 
                                Power = Math.Round(Convert.ToDouble(values[0])/1000, 2),                  //A
                                Temp = Math.Round(Convert.ToDouble(values[1])/100, 2),                    //*C
                                Fuse = SetPowerErrors((List<bool>)values[2]) },                     //List of PowerError Strings ,

                152 => new { Type = "DATA5V", 
                                Power_temp = Math.Round(Convert.ToDouble(values[0])/100, 2) },            //*C

                _ => new { Type = "Unknown", CanID = canId, RawData = values } // Default if CAN ID not recognized
            };
        }
        private List<string> SetPowerErrors(List<bool> errors)
        {
            List<string> PowerErrors = new List<string> { "OverCurrent Trip", "Fuse Fault", "OverTemp Fuse" };
            List<string> result = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                if (errors[i])
                {
                    result.Add(PowerErrors[i]);
                }
            }
            return result;
        }
        private List<string> SetErrors(List<bool> errors, int x)
        {

            List<List<string>> errorTypes = new List<List<string>>
            {
                new List<string> { "HAL_ERROR", "HAL_BUSY", "HAL_TIMEOUT", "INIT_ERROR", "WHO_AM_I_ERROR", "MEMS_ERROR", "MAG_WHO_AM_I_ERROR" },
                new List<string> { "HAL_ERROR", "HAL_BUSY", "HAL_TIMEOUT" },
                new List<string> { "HAL_ERROR", "HAL_BUSY", "HAL_TIMEOUT" },
                new List<string> { "Probe_1", "Probe_2", "Probe_3", "Probe_4" }
            };

            // Check if x is within valid range
            if (x < 0 || x >= errorTypes.Count)
            {
                // Handle invalid x (perhaps by returning an empty list or logging an error)
                return new List<string>();
            }

            List<string> selectedErrors = errorTypes[x];
            List<string> result = new List<string>();

            for (int i = 0; i < errors.Count; i++)
            {
                if (errors[i])
                {
                    result.Add(selectedErrors[i]);
                }
            }
            return result;
        }
        // Helper function to convert JSON arrays properly
        public static List<object> ConvertJsonData(JsonElement jsonElement)
        {
            List<object> outerList = new();

            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException("Expected JSON array at the root level.");
            }

            foreach (var element in jsonElement.EnumerateArray()) // ✅ Iterate properly
            {
                outerList.Add(ConvertElement(element));
            }

            return outerList;
        }

        private static object ConvertElement(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                List<object> nestedList = element.EnumerateArray()
                    .Select(ConvertElement)
                    .ToList();

                return EnsureConsistentListType(nestedList);
            }

            return ConvertJsonValue(element);
        }

        private static object ConvertJsonValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Number => element.TryGetInt32(out int intValue) ? intValue : element.GetDouble(),
                JsonValueKind.True or JsonValueKind.False => element.GetBoolean(),
                JsonValueKind.String => element.GetString(),
                _ => throw new Exception($"Unhandled JSON type: {element.ValueKind}")
            };
        }

        private static object EnsureConsistentListType(List<object> list)
        {
            if (list.Count == 0) return list;

            // Identify the first non-null element type
            Type? firstType = list.FirstOrDefault(x => x != null)?.GetType();
            if (firstType == null) return list; // Return empty list if all elements are null

            // Ensure all elements are of the same type
            if (list.All(x => x?.GetType() == firstType))
            {
                // Now create the correct type of list and populate it
                if (firstType == typeof(int))
                {
                    List<int> typedList = new();
                    foreach (var item in list) typedList.Add(Convert.ToInt32(item));
                    return typedList;
                }
                if (firstType == typeof(double))
                {
                    List<double> typedList = new();
                    foreach (var item in list) typedList.Add(Convert.ToDouble(item));
                    return typedList;
                }
                if (firstType == typeof(bool))
                {
                    List<bool> typedList = new();
                    foreach (var item in list) typedList.Add((bool)item);
                    return typedList;
                }
                if (firstType == typeof(string))
                {
                    List<string> typedList = new();
                    foreach (var item in list) typedList.Add(item.ToString()!);
                    return typedList;
                }
            }

            return list; // Return as List<object> if mixed types are present
        }
    }
}
