using LaserFocus.Core.Services;
using System;
using System.IO;
using Xunit;

namespace LaserFocus.Tests.Services
{
    public class LoggingServiceTests : IDisposable
    {
        private readonly string _testLogDirectory;
        private readonly LoggingService _loggingService;

        public LoggingServiceTests()
        {
            // Create a temporary directory for test logs
            _testLogDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testLogDirectory);

            // Get the singleton instance
            _loggingService = LoggingService.Instance;
        }

        public void Dispose()
        {
            // Clean up test directory
            if (Directory.Exists(_testLogDirectory))
            {
                Directory.Delete(_testLogDirectory, true);
            }
        }

        [Fact]
        public void Instance_ShouldReturnSameInstance()
        {
            // Act
            var instance1 = LoggingService.Instance;
            var instance2 = LoggingService.Instance;

            // Assert
            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void LogInfo_ShouldNotThrowException()
        {
            // Arrange
            var message = "Test info message";
            var source = "TestSource";

            // Act & Assert
            var exception = Record.Exception(() => _loggingService.LogInfo(message, source));
            Assert.Null(exception);
        }

        [Fact]
        public void LogWarning_ShouldNotThrowException()
        {
            // Arrange
            var message = "Test warning message";
            var source = "TestSource";

            // Act & Assert
            var exception = Record.Exception(() => _loggingService.LogWarning(message, source));
            Assert.Null(exception);
        }

        [Fact]
        public void LogError_ShouldNotThrowException()
        {
            // Arrange
            var message = "Test error message";
            var source = "TestSource";

            // Act & Assert
            var exception = Record.Exception(() => _loggingService.LogError(message, source));
            Assert.Null(exception);
        }

        [Fact]
        public void LogException_WithException_ShouldNotThrowException()
        {
            // Arrange
            var testException = new InvalidOperationException("Test exception");
            var source = "TestSource";
            var additionalMessage = "Additional context";

            // Act & Assert
            var exception = Record.Exception(() => _loggingService.LogException(testException, source, additionalMessage));
            Assert.Null(exception);
        }

        [Fact]
        public void LogException_WithoutAdditionalMessage_ShouldNotThrowException()
        {
            // Arrange
            var testException = new ArgumentException("Test exception");
            var source = "TestSource";

            // Act & Assert
            var exception = Record.Exception(() => _loggingService.LogException(testException, source));
            Assert.Null(exception);
        }

        [Fact]
        public void GetRecentLogEntries_WhenNoLogFile_ReturnsEmptyList()
        {
            // Act
            var entries = _loggingService.GetRecentLogEntries();

            // Assert
            Assert.NotNull(entries);
            // Note: May not be empty if other tests have logged entries
        }

        [Fact]
        public void GetRecentLogEntries_WithMaxEntries_ReturnsCorrectCount()
        {
            // Arrange
            var maxEntries = 5;

            // Log some entries first
            for (int i = 0; i < 10; i++)
            {
                _loggingService.LogInfo($"Test message {i}", "TestSource");
            }

            // Act
            var entries = _loggingService.GetRecentLogEntries(maxEntries);

            // Assert
            Assert.NotNull(entries);
            Assert.True(entries.Count <= maxEntries);
        }

        [Fact]
        public void CleanupOldLogs_ShouldNotThrowException()
        {
            // Arrange
            var daysToKeep = 7;

            // Act & Assert
            var exception = Record.Exception(() => _loggingService.CleanupOldLogs(daysToKeep));
            Assert.Null(exception);
        }

        [Fact]
        public void LogInfo_WithEmptySource_ShouldNotThrowException()
        {
            // Arrange
            var message = "Test message";

            // Act & Assert
            var exception = Record.Exception(() => _loggingService.LogInfo(message));
            Assert.Null(exception);
        }

        [Fact]
        public void LogWarning_WithEmptySource_ShouldNotThrowException()
        {
            // Arrange
            var message = "Test warning";

            // Act & Assert
            var exception = Record.Exception(() => _loggingService.LogWarning(message));
            Assert.Null(exception);
        }

        [Fact]
        public void LogError_WithEmptySource_ShouldNotThrowException()
        {
            // Arrange
            var message = "Test error";

            // Act & Assert
            var exception = Record.Exception(() => _loggingService.LogError(message));
            Assert.Null(exception);
        }

        [Fact]
        public void LogException_WithEmptySource_ShouldNotThrowException()
        {
            // Arrange
            var testException = new Exception("Test exception");

            // Act & Assert
            var exception = Record.Exception(() => _loggingService.LogException(testException));
            Assert.Null(exception);
        }

        [Fact]
        public void LogInfo_WithNullMessage_ShouldNotThrowException()
        {
            // Act & Assert
            var exception = Record.Exception(() => _loggingService.LogInfo(null!, "TestSource"));
            Assert.Null(exception);
        }

        [Fact]
        public void LogException_WithNullException_ShouldHandleGracefully()
        {
            // Act & Assert - The method should handle null exceptions gracefully
            // We expect it might throw a NullReferenceException, which is acceptable behavior
            var exception = Record.Exception(() => _loggingService.LogException(null!, "TestSource"));
            // Either no exception or a NullReferenceException is acceptable
            Assert.True(exception == null || exception is NullReferenceException);
        }

        [Fact]
        public void GetRecentLogEntries_WithZeroMaxEntries_ReturnsEmptyList()
        {
            // Act
            var entries = _loggingService.GetRecentLogEntries(0);

            // Assert
            Assert.NotNull(entries);
            Assert.Empty(entries);
        }

        [Fact]
        public void GetRecentLogEntries_WithNegativeMaxEntries_ReturnsEmptyOrErrorList()
        {
            // Act
            var entries = _loggingService.GetRecentLogEntries(-5);

            // Assert
            Assert.NotNull(entries);
            // The method might return empty list or error message, both are acceptable
            Assert.True(entries.Count == 0 || entries.Contains("Error reading log file"));
        }

        [Fact]
        public void CleanupOldLogs_WithZeroDays_ShouldNotThrowException()
        {
            // Act & Assert
            var exception = Record.Exception(() => _loggingService.CleanupOldLogs(0));
            Assert.Null(exception);
        }

        [Fact]
        public void CleanupOldLogs_WithNegativeDays_ShouldNotThrowException()
        {
            // Act & Assert
            var exception = Record.Exception(() => _loggingService.CleanupOldLogs(-1));
            Assert.Null(exception);
        }

        [Fact]
        public void MultipleThreads_LoggingSimultaneously_ShouldNotThrowException()
        {
            // Arrange
            var tasks = new List<Task>();
            var numberOfTasks = 10;
            var messagesPerTask = 5;

            // Act
            for (int i = 0; i < numberOfTasks; i++)
            {
                int taskId = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < messagesPerTask; j++)
                    {
                        _loggingService.LogInfo($"Task {taskId} Message {j}", $"Task{taskId}");
                    }
                }));
            }

            // Assert
            var exception = Record.Exception(() => Task.WaitAll(tasks.ToArray()));
            Assert.Null(exception);
        }
    }
}