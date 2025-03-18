

using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace Backend.Logging
{
    public class LoggerService
    {
        private readonly ILogger _dataLogger;
        private readonly string _logFolder;

        public LoggerService(ILoggerFactory loggerFactory)
        {
            //Define the logs folder
            _logFolder = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            Directory.CreateDirectory(_logFolder); // Ensure directory exists

            _dataLogger = loggerFactory.CreateLogger("DataLogger");
        }


        //Function to read the latest logs and return as a string
        public string GetFilteredLogs(string filter)
{
    if (!Directory.Exists(_logFolder))
        return "Log directory does not exist.";

    var logFiles = Directory.GetFiles(_logFolder, "*.log")
        .OrderByDescending(File.GetCreationTime) //Sort files so latest is first
        .ToList();

    if (!logFiles.Any())
        return "No log files found.";

    DateTime startDate = filter switch
    {
        "last7days" => DateTime.Now.AddDays(-7),
        "last30days" => DateTime.Now.AddDays(-30),
        _ => DateTime.Today
    };

    // Read logs from selected date range
    var filteredLogs = logFiles
        .Where(file => File.GetCreationTime(file) >= startDate)
        .SelectMany(file => 
        {
            var lines = File.ReadAllLines(file).Reverse().ToList(); // Read & Reverse
            return lines;
        })
        .ToList();


    return filteredLogs.Any() ? string.Join("\n", filteredLogs) : "No logs matching the selected date range.";
}






        //Generic function for writing logs (Ensures thread safety)
        private void WriteLog(string logType, string message)
        {
            string logFile = Path.Combine(_logFolder, $"{DateTime.Now:yyyy-MM-dd}.log");
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {logType}: {message}";

            try
            {
                lock (this)  // Ensures thread-safe writes
                {
                    File.AppendAllLines(logFile, new[] { logEntry });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR WRITING TO LOG FILE: {ex.Message}");
            }

            // Log internally based on log type
            switch (logType)
            {
                case "INFO":
                    _dataLogger.LogInformation(logEntry);
                    break;
                case "WARNING":
                    _dataLogger.LogWarning(logEntry);
                    break;
                case "ERROR":
                    _dataLogger.LogError(logEntry);
                    break;
            }
        }

        public void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public void LogWarning(string message)
        {
            WriteLog("WARNING", message);
        }

        public void LogError(string message)
        {
            WriteLog("ERROR", message);
        }
    }
}
