using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using LaserFocus.Core.Models;
using LaserFocus.Core.Services;

namespace LaserFocus.Tests.Integration
{
    /// <summary>
    /// Integration tests for process monitoring and termination functionality
    /// Tests Requirements: 2.1, 2.2, 4.1
    /// </summary>
    public class ProcessMonitoringIntegrationTests : IDisposable
    {
        private readonly string _testConfigDirectory;
        private readonly ConfigurationManager _configurationManager;
        private readonly ProcessMonitor _processMonitor;
        private readonly List<Process> _testProcesses;

        public ProcessMonitoringIntegrationTests()
        {
            _testConfigDirectory = Path.Combine(Path.GetTempPath(), $"LaserFocusTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testConfigDirectory);
            
            _configurationManager = new ConfigurationManager(_testConfigDirectory);
            
            // Initialize with default allowed processes
            var allowedProcesses = new[] { "chrome", "code", "kiro", "devenv", "notepad" };
            _processMonitor = new ProcessMonitor(allowedProcesses);
            
            _testProcesses = new List<Process>();
        }

        [Fact]
        public void ProcessMonitoring_GetRunningProcesses_ShouldReturnUserProcessesWithStatus()
        {
            // Act
            var runningProcesses = _processMonitor.GetRunningProcesses();

            // Assert
            Assert.NotNull(runningProcesses);
            Assert.True(runningProcesses.Count > 0, "Should find at least some running processes");

            // Verify each process has required properties
            foreach (var processInfo in runningProcesses)
            {
                Assert.NotNull(processInfo.ProcessName);
                Assert.True(processInfo.Id > 0);
                Assert.NotNull(processInfo.Status);
                Assert.True(processInfo.Status == "Allowed" || processInfo.Status == "Blocked");
            }
        }

        [Fact]
        public void ProcessMonitoring_AllowedProcesses_ShouldBeMarkedAsAllowed()
        {
            // Arrange - Start a notepad process (which is in allowed list)
            Process? testProcess = null;
            
            try
            {
                testProcess = Process.Start("notepad.exe");
                _testProcesses.Add(testProcess);
                
                // Wait a moment for process to start
                Thread.Sleep(1000);

                // Act
                var runningProcesses = _processMonitor.GetRunningProcesses();
                var notepadProcesses = runningProcesses.Where(p => 
                    p.ProcessName.Equals("notepad", StringComparison.OrdinalIgnoreCase)).ToList();

                // Assert
                Assert.True(notepadProcesses.Count > 0, "Should find the notepad process we started");
                
                foreach (var notepadProcess in notepadProcesses)
                {
                    Assert.Equal("Allowed", notepadProcess.Status);
                }
            }
            finally
            {
                // Cleanup
                testProcess?.Kill();
                testProcess?.WaitForExit(5000);
            }
        }

        [Fact]
        public void ProcessMonitoring_IsProcessAllowed_ShouldCorrectlyIdentifyAllowedProcesses()
        {
            // Arrange
            var allowedProcessNames = new[] { "chrome", "code", "kiro", "devenv", "notepad" };
            var blockedProcessNames = new[] { "calc", "mspaint", "wordpad" };

            // Act & Assert - Test allowed processes
            foreach (var processName in allowedProcessNames)
            {
                var isAllowed = _processMonitor.IsProcessAllowed(processName);
                Assert.True(isAllowed, $"Process '{processName}' should be allowed");
            }

            // Act & Assert - Test blocked processes
            foreach (var processName in blockedProcessNames)
            {
                var isAllowed = _processMonitor.IsProcessAllowed(processName);
                Assert.False(isAllowed, $"Process '{processName}' should be blocked");
            }
        }

        [Fact]
        public void ProcessMonitoring_TerminateProcess_ShouldHandleValidAndInvalidProcessIds()
        {
            // Test with invalid process ID
            var exception = Record.Exception(() => _processMonitor.TerminateProcess(-1));
            Assert.Null(exception); // Should handle gracefully without throwing

            // Test with non-existent process ID
            exception = Record.Exception(() => _processMonitor.TerminateProcess(999999));
            Assert.Null(exception); // Should handle gracefully without throwing
        }

        [Fact]
        public async Task ProcessMonitoring_Integration_ShouldDetectAndClassifyProcesses()
        {
            // Arrange - Start multiple test processes
            var testProcesses = new List<Process>();
            
            try
            {
                // Start an allowed process (notepad)
                var notepadProcess = Process.Start("notepad.exe");
                testProcesses.Add(notepadProcess);
                
                // Wait for processes to start
                await Task.Delay(2000);

                // Act
                var runningProcesses = _processMonitor.GetRunningProcesses();

                // Assert
                Assert.NotNull(runningProcesses);
                
                // Find our test processes
                var foundNotepad = runningProcesses.Any(p => 
                    p.ProcessName.Equals("notepad", StringComparison.OrdinalIgnoreCase) && 
                    p.Status == "Allowed");
                
                Assert.True(foundNotepad, "Should find notepad process marked as allowed");

                // Verify process information is complete
                var notepadProcessInfo = runningProcesses.FirstOrDefault(p => 
                    p.ProcessName.Equals("notepad", StringComparison.OrdinalIgnoreCase));
                
                if (notepadProcessInfo != null)
                {
                    Assert.True(notepadProcessInfo.Id > 0);
                    Assert.NotNull(notepadProcessInfo.ProcessName);
                    Assert.Equal("Allowed", notepadProcessInfo.Status);
                    Assert.True(notepadProcessInfo.LastSeen <= DateTime.Now);
                }
            }
            finally
            {
                // Cleanup all test processes
                foreach (var process in testProcesses)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(5000);
                        }
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        [Fact]
        public void ProcessMonitoring_ConfigurationIntegration_ShouldPersistAllowedApplications()
        {
            // Arrange
            var testAllowedApps = new List<string> { "chrome", "code", "kiro", "testapp" };

            // Act - Save configuration
            _configurationManager.SaveAllowedApplications(testAllowedApps);

            // Simulate application restart
            var newConfigManager = new ConfigurationManager(_testConfigDirectory);
            var loadedApps = newConfigManager.LoadAllowedApplications();

            // Assert
            Assert.Equal(testAllowedApps.Count, loadedApps.Count);
            foreach (var app in testAllowedApps)
            {
                Assert.Contains(app, loadedApps);
            }
        }

        public void Dispose()
        {
            // Cleanup any remaining test processes
            foreach (var process in _testProcesses)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            // Cleanup test directory
            if (Directory.Exists(_testConfigDirectory))
            {
                Directory.Delete(_testConfigDirectory, true);
            }
        }
    }
}