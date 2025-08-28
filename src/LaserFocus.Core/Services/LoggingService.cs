using System;
using System.Collections.Generic;
using System.IO;

namespace LaserFocus.Core.Services
{
    /// <summary>
    /// Provides centralized logging functionality for debugging and troubleshooting
    /// </summary>
    public class LoggingService
    {
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();
        private static LoggingService? _instance;
        private static readonly object _instanceLock = new object();

        /// <summary>
        /// Gets the singleton instance of LoggingService
        /// </summary>
        public static LoggingService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        _instance ??= new LoggingService();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initializes a new instance of LoggingService
        /// </summary>
        private LoggingService()
        {
            var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            
            try
            {
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
            }
            catch
            {
                // If we can't create logs directory, use temp directory
                logDirectory = Path.GetTempPath();
            }

            _logFilePath = Path.Combine(logDirectory, $"LaserFocus_{DateTime.Now:yyyyMMdd}.log");
        }

        /// <summary>
        /// Logs an informational message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="source">The source component or method name</param>
        public void LogInfo(string message, string source = "")
        {
            WriteLog("INFO", message, source);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="source">The source component or method name</param>
        public void LogWarning(string message, string source = "")
        {
            WriteLog("WARN", message, source);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="source">The source component or method name</param>
        public void LogError(string message, string source = "")
        {
            WriteLog("ERROR", message, source);
        }

        /// <summary>
        /// Logs an exception with full details
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="source">The source component or method name</param>
        /// <param name="additionalMessage">Additional context message</param>
        public void LogException(Exception exception, string source = "", string additionalMessage = "")
        {
            var message = string.IsNullOrEmpty(additionalMessage) 
                ? $"Exception: {exception.Message}" 
                : $"{additionalMessage} - Exception: {exception.Message}";
            
            if (!string.IsNullOrEmpty(exception.StackTrace))
            {
                message += $"\nStack Trace: {exception.StackTrace}";
            }

            WriteLog("ERROR", message, source);
        }

        /// <summary>
        /// Writes a log entry to the log file
        /// </summary>
        /// <param name="level">Log level (INFO, WARN, ERROR)</param>
        /// <param name="message">The message to log</param>
        /// <param name="source">The source component or method name</param>
        private void WriteLog(string level, string message, string source)
        {
            try
            {
                lock (_lockObject)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var sourceInfo = string.IsNullOrEmpty(source) ? "" : $" [{source}]";
                    var logEntry = $"{timestamp} {level}{sourceInfo}: {message}";

                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }
            }
            catch
            {
                // Silently fail if logging fails to prevent cascading errors
            }
        }

        /// <summary>
        /// Gets recent log entries for debugging purposes
        /// </summary>
        /// <param name="maxEntries">Maximum number of entries to return</param>
        /// <returns>List of recent log entries</returns>
        public List<string> GetRecentLogEntries(int maxEntries = 50)
        {
            try
            {
                lock (_lockObject)
                {
                    if (!File.Exists(_logFilePath))
                    {
                        return new List<string>();
                    }

                    var lines = File.ReadAllLines(_logFilePath);
                    var recentLines = lines.Length <= maxEntries 
                        ? lines 
                        : lines[(lines.Length - maxEntries)..];

                    return new List<string>(recentLines);
                }
            }
            catch
            {
                return new List<string> { "Error reading log file" };
            }
        }

        /// <summary>
        /// Clears old log files to prevent disk space issues
        /// </summary>
        /// <param name="daysToKeep">Number of days of logs to keep</param>
        public void CleanupOldLogs(int daysToKeep = 7)
        {
            try
            {
                var logDirectory = Path.GetDirectoryName(_logFilePath);
                if (string.IsNullOrEmpty(logDirectory) || !Directory.Exists(logDirectory))
                {
                    return;
                }

                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var logFiles = Directory.GetFiles(logDirectory, "LaserFocus_*.log");

                foreach (var logFile in logFiles)
                {
                    var fileInfo = new FileInfo(logFile);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(logFile);
                    }
                }
            }
            catch
            {
                // Silently fail if cleanup fails
            }
        }
    }
}