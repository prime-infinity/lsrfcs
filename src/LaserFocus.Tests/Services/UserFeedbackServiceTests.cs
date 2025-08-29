using LaserFocus.Core.Services;
using System;
using System.IO;
using System.Security;
using Xunit;

namespace LaserFocus.Tests.Services
{
    public class UserFeedbackServiceTests
    {
        [Fact]
        public void GetFriendlyErrorMessage_UnauthorizedAccessException_ReturnsAdminMessage()
        {
            // Arrange
            var exception = new UnauthorizedAccessException("Access denied");

            // Act
            var message = UserFeedbackHelper.GetFriendlyErrorMessage(exception);

            // Assert
            Assert.Contains("Administrator privileges are required", message);
            Assert.Contains("restart the application as administrator", message);
        }

        [Fact]
        public void GetFriendlyErrorMessage_FileNotFoundException_ReturnsFileMessage()
        {
            // Arrange
            var exception = new FileNotFoundException("File not found");

            // Act
            var message = UserFeedbackHelper.GetFriendlyErrorMessage(exception);

            // Assert
            Assert.Contains("required file could not be found", message);
            Assert.Contains("reinstalled", message);
        }

        [Fact]
        public void GetFriendlyErrorMessage_DirectoryNotFoundException_ReturnsDirectoryMessage()
        {
            // Arrange
            var exception = new DirectoryNotFoundException("Directory not found");

            // Act
            var message = UserFeedbackHelper.GetFriendlyErrorMessage(exception);

            // Assert
            Assert.Contains("required directory could not be found", message);
            Assert.Contains("properly installed", message);
        }

        [Fact]
        public void GetFriendlyErrorMessage_IOException_ReturnsFileOperationMessage()
        {
            // Arrange
            var exception = new IOException("IO error");

            // Act
            var message = UserFeedbackHelper.GetFriendlyErrorMessage(exception);

            // Assert
            Assert.Contains("file operation failed", message);
            Assert.Contains("in use by another program", message);
        }

        [Fact]
        public void GetFriendlyErrorMessage_ArgumentException_ReturnsInputMessage()
        {
            // Arrange
            var exception = new ArgumentException("Invalid argument");

            // Act
            var message = UserFeedbackHelper.GetFriendlyErrorMessage(exception);

            // Assert
            Assert.Contains("Invalid input provided", message);
            Assert.Contains("check your input", message);
        }

        [Fact]
        public void GetFriendlyErrorMessage_InvalidOperationException_ReturnsOperationMessage()
        {
            // Arrange
            var exception = new InvalidOperationException("Invalid operation");

            // Act
            var message = UserFeedbackHelper.GetFriendlyErrorMessage(exception);

            // Assert
            Assert.Contains("cannot be performed at this time", message);
            Assert.Contains("try again later", message);
        }

        [Fact]
        public void GetFriendlyErrorMessage_SecurityException_ReturnsSecurityMessage()
        {
            // Arrange
            var exception = new SecurityException("Security error");

            // Act
            var message = UserFeedbackHelper.GetFriendlyErrorMessage(exception);

            // Assert
            Assert.Contains("Security restrictions", message);
            Assert.Contains("Administrator privileges may be required", message);
        }

        [Fact]
        public void GetFriendlyErrorMessage_TimeoutException_ReturnsTimeoutMessage()
        {
            // Arrange
            var exception = new TimeoutException("Operation timed out");

            // Act
            var message = UserFeedbackHelper.GetFriendlyErrorMessage(exception);

            // Assert
            Assert.Contains("operation timed out", message);
            Assert.Contains("system resources", message);
        }

        [Fact]
        public void GetFriendlyErrorMessage_OutOfMemoryException_ReturnsMemoryMessage()
        {
            // Arrange
            var exception = new OutOfMemoryException("Out of memory");

            // Act
            var message = UserFeedbackHelper.GetFriendlyErrorMessage(exception);

            // Assert
            Assert.Contains("running low on memory", message);
            Assert.Contains("close other applications", message);
        }

        [Fact]
        public void GetFriendlyErrorMessage_UnknownException_ReturnsGenericMessage()
        {
            // Arrange
            var exception = new NotImplementedException("Not implemented");

            // Act
            var message = UserFeedbackHelper.GetFriendlyErrorMessage(exception);

            // Assert
            Assert.Contains("unexpected error occurred", message);
            Assert.Contains("application logs", message);
        }

        [Fact]
        public void GetFriendlyErrorMessage_NullException_ReturnsGenericMessage()
        {
            // Arrange
            Exception? exception = null;

            // Act
            var message = UserFeedbackHelper.GetFriendlyErrorMessage(exception!);

            // Assert
            Assert.Contains("unexpected error occurred", message);
        }

        [Fact]
        public void GetFriendlyErrorMessage_ExceptionWithInnerException_ReturnsMessageForOuterException()
        {
            // Arrange
            var innerException = new FileNotFoundException("Inner exception");
            var outerException = new InvalidOperationException("Outer exception", innerException);

            // Act
            var message = UserFeedbackHelper.GetFriendlyErrorMessage(outerException);

            // Assert
            Assert.Contains("cannot be performed at this time", message);
            Assert.DoesNotContain("required file could not be found", message);
        }

        [Theory]
        [InlineData(typeof(UnauthorizedAccessException))]
        [InlineData(typeof(FileNotFoundException))]
        [InlineData(typeof(DirectoryNotFoundException))]
        [InlineData(typeof(IOException))]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(InvalidOperationException))]
        [InlineData(typeof(SecurityException))]
        [InlineData(typeof(TimeoutException))]
        [InlineData(typeof(OutOfMemoryException))]
        public void GetFriendlyErrorMessage_KnownExceptionTypes_ReturnsNonEmptyMessage(Type exceptionType)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType, "Test message")!;

            // Act
            var message = UserFeedbackHelper.GetFriendlyErrorMessage(exception);

            // Assert
            Assert.NotNull(message);
            Assert.NotEmpty(message);
            Assert.True(message.Length > 10); // Ensure it's a meaningful message
        }

        [Fact]
        public void GetFriendlyErrorMessage_AllKnownExceptions_ReturnUniqueMessages()
        {
            // Arrange
            var exceptions = new Exception[]
            {
                new UnauthorizedAccessException(),
                new FileNotFoundException(),
                new DirectoryNotFoundException(),
                new IOException(),
                new ArgumentException(),
                new InvalidOperationException(),
                new SecurityException(),
                new TimeoutException(),
                new OutOfMemoryException()
            };

            var messages = new HashSet<string>();

            // Act & Assert
            foreach (var exception in exceptions)
            {
                var message = UserFeedbackHelper.GetFriendlyErrorMessage(exception);
                Assert.True(messages.Add(message), $"Duplicate message found for {exception.GetType().Name}: {message}");
            }
        }

        [Fact]
        public void GetFriendlyErrorMessage_ConsistentResults_SameExceptionTypesReturnSameMessage()
        {
            // Arrange
            var exception1 = new UnauthorizedAccessException("Message 1");
            var exception2 = new UnauthorizedAccessException("Message 2");

            // Act
            var message1 = UserFeedbackHelper.GetFriendlyErrorMessage(exception1);
            var message2 = UserFeedbackHelper.GetFriendlyErrorMessage(exception2);

            // Assert
            Assert.Equal(message1, message2);
        }
    }

    /// <summary>
    /// Mock implementation of IUserFeedbackService for testing purposes
    /// </summary>
    public class MockUserFeedbackService : IUserFeedbackService
    {
        public List<string> Messages { get; } = new();
        public List<string> Titles { get; } = new();
        public List<string> MessageTypes { get; } = new();
        public bool ConfirmationResult { get; set; } = true;

        public void ShowSuccess(string message, string title = "Success")
        {
            Messages.Add(message);
            Titles.Add(title);
            MessageTypes.Add("Success");
        }

        public void ShowError(string message, string title = "Error", Exception? exception = null)
        {
            Messages.Add(message);
            Titles.Add(title);
            MessageTypes.Add("Error");
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            Messages.Add(message);
            Titles.Add(title);
            MessageTypes.Add("Warning");
        }

        public bool ShowConfirmation(string message, string title = "Confirmation")
        {
            Messages.Add(message);
            Titles.Add(title);
            MessageTypes.Add("Confirmation");
            return ConfirmationResult;
        }

        public void ShowInfo(string message, string title = "Information")
        {
            Messages.Add(message);
            Titles.Add(title);
            MessageTypes.Add("Info");
        }

        public void ShowDetailedError(string userMessage, string technicalDetails, string title = "Error Details")
        {
            Messages.Add($"{userMessage}\n{technicalDetails}");
            Titles.Add(title);
            MessageTypes.Add("DetailedError");
        }

        public string GetFriendlyErrorMessage(Exception exception)
        {
            return UserFeedbackHelper.GetFriendlyErrorMessage(exception);
        }

        public void Clear()
        {
            Messages.Clear();
            Titles.Clear();
            MessageTypes.Clear();
        }
    }

    public class MockUserFeedbackServiceTests
    {
        [Fact]
        public void MockUserFeedbackService_ShowSuccess_RecordsMessage()
        {
            // Arrange
            var service = new MockUserFeedbackService();
            var message = "Operation successful";
            var title = "Success";

            // Act
            service.ShowSuccess(message, title);

            // Assert
            Assert.Single(service.Messages);
            Assert.Equal(message, service.Messages[0]);
            Assert.Equal(title, service.Titles[0]);
            Assert.Equal("Success", service.MessageTypes[0]);
        }

        [Fact]
        public void MockUserFeedbackService_ShowConfirmation_ReturnsConfiguredResult()
        {
            // Arrange
            var service = new MockUserFeedbackService { ConfirmationResult = false };
            var message = "Are you sure?";

            // Act
            var result = service.ShowConfirmation(message);

            // Assert
            Assert.False(result);
            Assert.Single(service.Messages);
            Assert.Equal(message, service.Messages[0]);
        }

        [Fact]
        public void MockUserFeedbackService_Clear_RemovesAllRecords()
        {
            // Arrange
            var service = new MockUserFeedbackService();
            service.ShowSuccess("Test");
            service.ShowError("Error");

            // Act
            service.Clear();

            // Assert
            Assert.Empty(service.Messages);
            Assert.Empty(service.Titles);
            Assert.Empty(service.MessageTypes);
        }
    }
}