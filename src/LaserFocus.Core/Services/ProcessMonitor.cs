using System.Diagnostics;
using LaserFocus.Core.Models;

namespace LaserFocus.Core.Services
{
    /// <summary>
    /// Monitors running processes and manages application control by terminating non-allowed applications
    /// </summary>
    public class ProcessMonitor
    {
        private readonly string[] _allowedProcesses;
        private readonly List<string> _processActionLog;

        /// <summary>
        /// Initializes a new instance of ProcessMonitor with the specified allowed processes
        /// </summary>
        /// <param name="allowedProcesses">Array of process names that are allowed to run</param>
        public ProcessMonitor(string[] allowedProcesses)
        {
            _allowedProcesses = allowedProcesses ?? throw new ArgumentNullException(nameof(allowedProcesses));
            _processActionLog = new List<string>();
        }

        /// <summary>
        /// Gets a list of currently running user processes with their status information
        /// </summary>
        /// <returns>List of ProcessInfo objects representing running user processes</returns>
        public List<ProcessInfo> GetRunningProcesses()
        {
            var processInfoList = new List<ProcessInfo>();

            try
            {
                var processes = Process.GetProcesses();
                
                foreach (var process in processes)
                {
                    try
                    {
                        // Only include user processes (skip system processes)
                        if (IsUserProcess(process))
                        {
                            var processInfo = new ProcessInfo
                            {
                                ProcessName = process.ProcessName,
                                Id = process.Id,
                                LastSeen = DateTime.Now
                            };

                            // Determine if process is allowed or blocked
                            if (IsProcessAllowed(process.ProcessName))
                            {
                                processInfo.Status = "Allowed";
                            }
                            else
                            {
                                processInfo.Status = "Blocked";
                            }

                            processInfoList.Add(processInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue processing other processes
                        LogProcessAction("Error", $"Failed to process {process.ProcessName}: {ex.Message}");
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                LogProcessAction("Error", $"Failed to enumerate processes: {ex.Message}");
            }

            return processInfoList;
        }

        /// <summary>
        /// Terminates a process by its process ID with proper error handling
        /// </summary>
        /// <param name="processId">The ID of the process to terminate</param>
        /// <returns>True if the process was successfully terminated, false otherwise</returns>
        public bool TerminateProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                var processName = process.ProcessName;

                // Don't terminate allowed processes
                if (IsProcessAllowed(processName))
                {
                    LogProcessAction("Skipped", $"Process {processName} (ID: {processId}) is allowed");
                    return false;
                }

                // Don't terminate critical system processes
                if (IsCriticalSystemProcess(processName))
                {
                    LogProcessAction("Skipped", $"Process {processName} (ID: {processId}) is a critical system process");
                    return false;
                }

                process.Kill();
                process.WaitForExit(5000); // Wait up to 5 seconds for process to exit
                
                LogProcessAction("Terminated", $"Process {processName} (ID: {processId}) was terminated");
                return true;
            }
            catch (ArgumentException)
            {
                // Process doesn't exist or has already exited
                LogProcessAction("Info", $"Process with ID {processId} no longer exists");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                LogProcessAction("Error", $"Cannot terminate process {processId}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                LogProcessAction("Error", $"Failed to terminate process {processId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a process name is in the allowed processes list
        /// </summary>
        /// <param name="processName">Name of the process to check</param>
        /// <returns>True if the process is allowed, false otherwise</returns>
        public bool IsProcessAllowed(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
                return false;

            return _allowedProcesses.Any(allowed => 
                string.Equals(allowed, processName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the process action log entries
        /// </summary>
        /// <returns>List of log entries</returns>
        public List<string> GetProcessActionLog()
        {
            return new List<string>(_processActionLog);
        }

        /// <summary>
        /// Clears the process action log
        /// </summary>
        public void ClearProcessActionLog()
        {
            _processActionLog.Clear();
        }

        /// <summary>
        /// Determines if a process is a user process (not a system process)
        /// </summary>
        /// <param name="process">The process to check</param>
        /// <returns>True if it's a user process, false if it's a system process</returns>
        private bool IsUserProcess(Process process)
        {
            try
            {
                // Skip processes without a main window title (typically system processes)
                if (string.IsNullOrEmpty(process.MainWindowTitle))
                    return false;

                // Skip processes that we can't access (typically system processes)
                var _ = process.ProcessName;
                var __ = process.Id;

                return true;
            }
            catch
            {
                // If we can't access process information, it's likely a system process
                return false;
            }
        }

        /// <summary>
        /// Checks if a process is a critical system process that should never be terminated
        /// </summary>
        /// <param name="processName">Name of the process to check</param>
        /// <returns>True if it's a critical system process, false otherwise</returns>
        private bool IsCriticalSystemProcess(string processName)
        {
            var criticalProcesses = new[]
            {
                "explorer", "winlogon", "csrss", "smss", "lsass", "services", "svchost",
                "dwm", "wininit", "system", "registry", "audiodg", "conhost"
            };

            return criticalProcesses.Any(critical => 
                string.Equals(critical, processName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Logs a process action with timestamp
        /// </summary>
        /// <param name="action">The action performed</param>
        /// <param name="processName">The process name or description</param>
        private void LogProcessAction(string action, string processName)
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {action}: {processName}";
            _processActionLog.Add(logEntry);

            // Keep only the last 100 log entries to prevent memory issues
            if (_processActionLog.Count > 100)
            {
                _processActionLog.RemoveAt(0);
            }
        }
    }
}