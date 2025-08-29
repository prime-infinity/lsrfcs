using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;

namespace LaserFocus.Core.Services
{
    /// <summary>
    /// Monitors and optimizes application performance including memory usage and resource management
    /// </summary>
    public class PerformanceMonitor
    {
        private readonly Dictionary<string, object> _performanceMetrics;
        private DateTime _lastOptimization;
        private readonly TimeSpan _optimizationInterval = TimeSpan.FromMinutes(5);

        public PerformanceMonitor()
        {
            _performanceMetrics = new Dictionary<string, object>();
            _lastOptimization = DateTime.Now;
            
            LoggingService.Instance.LogInfo("PerformanceMonitor initialized", "PerformanceMonitor");
        }

        /// <summary>
        /// Gets current memory usage statistics
        /// </summary>
        /// <returns>Dictionary containing memory usage information</returns>
        public Dictionary<string, object> GetMemoryUsage()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var memoryInfo = new Dictionary<string, object>
                {
                    ["WorkingSet"] = FormatBytes(process.WorkingSet64),
                    ["WorkingSetMB"] = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2),
                    ["PrivateMemory"] = FormatBytes(process.PrivateMemorySize64),
                    ["PrivateMemoryMB"] = Math.Round(process.PrivateMemorySize64 / 1024.0 / 1024.0, 2),
                    ["VirtualMemory"] = FormatBytes(process.VirtualMemorySize64),
                    ["VirtualMemoryMB"] = Math.Round(process.VirtualMemorySize64 / 1024.0 / 1024.0, 2),
                    ["GCTotalMemory"] = FormatBytes(GC.GetTotalMemory(false)),
                    ["GCTotalMemoryMB"] = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2),
                    ["Gen0Collections"] = GC.CollectionCount(0),
                    ["Gen1Collections"] = GC.CollectionCount(1),
                    ["Gen2Collections"] = GC.CollectionCount(2)
                };

                _performanceMetrics["LastMemoryCheck"] = DateTime.Now;
                return memoryInfo;
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "PerformanceMonitor.GetMemoryUsage", "Error getting memory usage");
                return new Dictionary<string, object> { ["Error"] = ex.Message };
            }
        }

        /// <summary>
        /// Gets current CPU usage information
        /// </summary>
        /// <returns>Dictionary containing CPU usage information</returns>
        public Dictionary<string, object> GetCpuUsage()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var cpuInfo = new Dictionary<string, object>
                {
                    ["ProcessorTime"] = process.TotalProcessorTime.TotalMilliseconds,
                    ["UserProcessorTime"] = process.UserProcessorTime.TotalMilliseconds,
                    ["PrivilegedProcessorTime"] = process.PrivilegedProcessorTime.TotalMilliseconds,
                    ["ThreadCount"] = process.Threads.Count,
                    ["HandleCount"] = process.HandleCount
                };

                return cpuInfo;
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "PerformanceMonitor.GetCpuUsage", "Error getting CPU usage");
                return new Dictionary<string, object> { ["Error"] = ex.Message };
            }
        }

        /// <summary>
        /// Performs memory optimization including garbage collection
        /// </summary>
        public void OptimizeMemory()
        {
            try
            {
                LoggingService.Instance.LogInfo("Starting memory optimization", "PerformanceMonitor.OptimizeMemory");
                
                var beforeMemory = GC.GetTotalMemory(false);
                
                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                // Compact Large Object Heap if available
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
                
                var afterMemory = GC.GetTotalMemory(false);
                var freedMemory = beforeMemory - afterMemory;
                
                _lastOptimization = DateTime.Now;
                _performanceMetrics["LastOptimization"] = _lastOptimization;
                _performanceMetrics["MemoryFreed"] = freedMemory;
                
                LoggingService.Instance.LogInfo($"Memory optimization completed. Freed: {FormatBytes(freedMemory)}", "PerformanceMonitor.OptimizeMemory");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "PerformanceMonitor.OptimizeMemory", "Error during memory optimization");
            }
        }

        /// <summary>
        /// Checks if automatic optimization should be performed
        /// </summary>
        /// <returns>True if optimization should be performed</returns>
        public bool ShouldOptimize()
        {
            return DateTime.Now - _lastOptimization > _optimizationInterval;
        }

        /// <summary>
        /// Performs automatic optimization if needed
        /// </summary>
        public void AutoOptimize()
        {
            if (ShouldOptimize())
            {
                OptimizeMemory();
            }
        }

        /// <summary>
        /// Gets comprehensive performance statistics
        /// </summary>
        /// <returns>Dictionary containing all performance metrics</returns>
        public Dictionary<string, object> GetPerformanceStats()
        {
            var stats = new Dictionary<string, object>();
            
            // Add memory usage
            var memoryStats = GetMemoryUsage();
            foreach (var kvp in memoryStats)
            {
                stats[$"Memory_{kvp.Key}"] = kvp.Value;
            }
            
            // Add CPU usage
            var cpuStats = GetCpuUsage();
            foreach (var kvp in cpuStats)
            {
                stats[$"CPU_{kvp.Key}"] = kvp.Value;
            }
            
            // Add performance metrics
            foreach (var kvp in _performanceMetrics)
            {
                stats[kvp.Key] = kvp.Value;
            }
            
            stats["Timestamp"] = DateTime.Now;
            
            return stats;
        }

        /// <summary>
        /// Logs current performance statistics
        /// </summary>
        public void LogPerformanceStats()
        {
            try
            {
                var stats = GetPerformanceStats();
                var memoryMB = stats.ContainsKey("Memory_WorkingSetMB") ? stats["Memory_WorkingSetMB"] : "Unknown";
                var gcMemoryMB = stats.ContainsKey("Memory_GCTotalMemoryMB") ? stats["Memory_GCTotalMemoryMB"] : "Unknown";
                var threadCount = stats.ContainsKey("CPU_ThreadCount") ? stats["CPU_ThreadCount"] : "Unknown";
                
                LoggingService.Instance.LogInfo($"Performance Stats - Memory: {memoryMB}MB, GC Memory: {gcMemoryMB}MB, Threads: {threadCount}", "PerformanceMonitor");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "PerformanceMonitor.LogPerformanceStats", "Error logging performance stats");
            }
        }

        /// <summary>
        /// Performs cleanup and resource disposal
        /// </summary>
        public void Cleanup()
        {
            try
            {
                LoggingService.Instance.LogInfo("Starting PerformanceMonitor cleanup", "PerformanceMonitor.Cleanup");
                
                // Log final performance stats
                LogPerformanceStats();
                
                // Perform final memory optimization
                OptimizeMemory();
                
                // Clear metrics
                _performanceMetrics.Clear();
                
                LoggingService.Instance.LogInfo("PerformanceMonitor cleanup completed", "PerformanceMonitor.Cleanup");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "PerformanceMonitor.Cleanup", "Error during cleanup");
            }
        }

        /// <summary>
        /// Formats bytes into human-readable format
        /// </summary>
        /// <param name="bytes">Number of bytes</param>
        /// <returns>Formatted string</returns>
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            
            return $"{number:n1} {suffixes[counter]}";
        }
    }
}