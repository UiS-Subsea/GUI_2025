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
            // Define the logs folder
            _logFolder = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (!Directory.Exists(_logFolder))
                Directory.CreateDirectory(_logFolder);

            _dataLogger = loggerFactory.CreateLogger("DataLogger");
        }

        // Function to read the latest logs and return as a string
        public string GetFilteredLogs(string filter)
        {
            if (!Directory.Exists(_logFolder))
        return "Log directory does not exist.";

        var logFiles = Directory.GetFiles(_logFolder, "*.log")
            .OrderByDescending(File.GetCreationTime) // Sort files by creation date (latest first)
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
            .SelectMany(file => File.ReadAllLines(file)) // Read all lines in file
            .Reverse() // Reverse order so newest logs appear first
            .ToList();

        return filteredLogs.Any() ? string.Join("\n", filteredLogs) : "No logs matching the selected date range.";
        }


        // Check if there is a log folder(creates if not) and saves the log file there
        public void LogInfo(string message)
        {
            if (!Directory.Exists(_logFolder))
                Directory.CreateDirectory(_logFolder);

            string logFile = Path.Combine(_logFolder, $"{DateTime.Now:yyyy-MM-dd}.log");
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} INFO: {message}{Environment.NewLine}";

            File.AppendAllText(logFile, logEntry);
            Console.WriteLine($"LOG: {logEntry}");
            _dataLogger.LogInformation(logEntry);
        }

    }
}
