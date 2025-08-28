using LaserFocus.Core.Services;
using LaserFocus.Core.Models;
using Xunit;

namespace LaserFocus.Tests.Services
{
    public class ProcessMonitorTests
    {
        private readonly ProcessMonitor _processMonitor;
        private readonly string[] _testAllowedProcesses = { "chrome", "code", "kiro" };

        public ProcessMonitorTests()
        {
            _processMonitor = new ProcessMonitor(_testAllowedProcesses);
        }

        [Fact]
        public void Constructor_WithValidAllowedProcesses_ShouldInitialize()
        {
            // Arrange & Act
            var monitor = new ProcessMonitor(_testAllowedProcesses);

            // Assert
            Assert.NotNull(monitor);
        }

        [Fact]
        public void Constructor_WithNullAllowedProcesses_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ProcessMonitor(null!));
        }

        [Fact]
        public void IsProcessAllowed_WithAllowedProcess_ShouldReturnTrue()
        {
            // Arrange
            var processName = "chrome";

            // Act
            var result = _processMonitor.IsProcessAllowed(processName);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsProcessAllowed_WithNonAllowedProcess_ShouldReturnFalse()
        {
            // Arrange
            var processName = "notepad";

            // Act
            var result = _processMonitor.IsProcessAllowed(processName);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsProcessAllowed_WithCaseInsensitiveMatch_ShouldReturnTrue()
        {
            // Arrange
            var processName = "CHROME";

            // Act
            var result = _processMonitor.IsProcessAllowed(processName);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsProcessAllowed_WithEmptyProcessName_ShouldReturnFalse()
        {
            // Arrange
            var processName = "";

            // Act
            var result = _processMonitor.IsProcessAllowed(processName);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsProcessAllowed_WithNullProcessName_ShouldReturnFalse()
        {
            // Arrange
            string? processName = null;

            // Act
            var result = _processMonitor.IsProcessAllowed(processName!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetRunningProcesses_ShouldReturnListOfProcessInfo()
        {
            // Act
            var processes = _processMonitor.GetRunningProcesses();

            // Assert
            Assert.NotNull(processes);
            Assert.IsType<List<ProcessInfo>>(processes);
        }

        [Fact]
        public void GetRunningProcesses_ShouldSetCorrectStatusForAllowedProcesses()
        {
            // Act
            var processes = _processMonitor.GetRunningProcesses();

            // Assert
            var allowedProcesses = processes.Where(p => _testAllowedProcesses.Contains(p.ProcessName.ToLower()));
            foreach (var process in allowedProcesses)
            {
                Assert.Equal("Allowed", process.Status);
            }
        }

        [Fact]
        public void GetRunningProcesses_ShouldSetCorrectStatusForBlockedProcesses()
        {
            // Act
            var processes = _processMonitor.GetRunningProcesses();

            // Assert
            var blockedProcesses = processes.Where(p => !_testAllowedProcesses.Contains(p.ProcessName.ToLower()));
            foreach (var process in blockedProcesses)
            {
                Assert.Equal("Blocked", process.Status);
            }
        }

        [Fact]
        public void GetRunningProcesses_ShouldSetLastSeenTimestamp()
        {
            // Arrange
            var beforeTime = DateTime.Now.AddSeconds(-1);

            // Act
            var processes = _processMonitor.GetRunningProcesses();
            var afterTime = DateTime.Now.AddSeconds(1);

            // Assert
            foreach (var process in processes)
            {
                Assert.True(process.LastSeen >= beforeTime && process.LastSeen <= afterTime);
            }
        }

        [Fact]
        public void TerminateProcess_WithNonExistentProcessId_ShouldReturnTrue()
        {
            // Arrange
            var nonExistentProcessId = -1;

            // Act
            var result = _processMonitor.TerminateProcess(nonExistentProcessId);

            // Assert
            Assert.True(result); // Should return true as process doesn't exist (goal achieved)
        }

        [Fact]
        public void GetProcessActionLog_InitiallyEmpty_ShouldReturnEmptyList()
        {
            // Act
            var log = _processMonitor.GetProcessActionLog();

            // Assert
            Assert.NotNull(log);
            Assert.Empty(log);
        }

        [Fact]
        public void ClearProcessActionLog_ShouldEmptyTheLog()
        {
            // Arrange
            // Trigger some logging by calling GetRunningProcesses
            _processMonitor.GetRunningProcesses();
            
            // Act
            _processMonitor.ClearProcessActionLog();
            var log = _processMonitor.GetProcessActionLog();

            // Assert
            Assert.Empty(log);
        }

        [Fact]
        public void GetProcessActionLog_ShouldReturnCopyOfLog()
        {
            // Arrange
            var originalLog = _processMonitor.GetProcessActionLog();
            
            // Act
            originalLog.Add("Test entry");
            var newLog = _processMonitor.GetProcessActionLog();

            // Assert
            Assert.NotSame(originalLog, newLog);
            Assert.DoesNotContain("Test entry", newLog);
        }

        [Fact]
        public void ProcessInfo_StatusChange_ShouldUpdateStatusColor()
        {
            // Arrange
            var processInfo = new ProcessInfo();

            // Act & Assert - Test Allowed status
            processInfo.Status = "Allowed";
            Assert.Equal("Green", processInfo.StatusColor);

            // Act & Assert - Test Blocked status
            processInfo.Status = "Blocked";
            Assert.Equal("Red", processInfo.StatusColor);

            // Act & Assert - Test unknown status
            processInfo.Status = "Unknown";
            Assert.Equal("Black", processInfo.StatusColor);
        }
    }
}