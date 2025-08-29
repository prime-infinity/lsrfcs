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
        public void Constructor_WithEmptyAllowedProcesses_ShouldWork()
        {
            // Arrange & Act
            var monitor = new ProcessMonitor(new string[0]);

            // Assert
            Assert.NotNull(monitor);
            Assert.False(monitor.IsProcessAllowed("chrome"));
        }

        [Theory]
        [InlineData("chrome@")]
        [InlineData("code#")]
        [InlineData("kiro$")]
        [InlineData("app.exe")]
        [InlineData("test-app")]
        public void IsProcessAllowed_WithSpecialCharacters_ShouldReturnFalse(string processName)
        {
            // Act
            var result = _processMonitor.IsProcessAllowed(processName);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("chrome.exe")]
        [InlineData("code.exe")]
        [InlineData("kiro.exe")]
        public void IsProcessAllowed_WithExeExtension_ShouldReturnFalse(string processName)
        {
            // Act
            var result = _processMonitor.IsProcessAllowed(processName);

            // Assert
            Assert.False(result); // Should not match with .exe extension
        }

        [Fact]
        public void IsProcessAllowed_WithPartialMatch_ShouldReturnFalse()
        {
            // Arrange - Test that partial matches don't work
            var partialProcessName = "chrom"; // Should not match "chrome"

            // Act
            var result = _processMonitor.IsProcessAllowed(partialProcessName);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsProcessAllowed_WithWhitespace_ShouldReturnFalse()
        {
            // Arrange
            var processNameWithSpaces = " chrome ";

            // Act
            var result = _processMonitor.IsProcessAllowed(processNameWithSpaces);

            // Assert
            Assert.False(result); // Should not match due to whitespace
        }
    }
}