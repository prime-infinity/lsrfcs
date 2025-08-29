using LaserFocus.Core.Models;
using System.ComponentModel;
using Xunit;

namespace LaserFocus.Tests.Models
{
    public class ProcessInfoTests
    {
        [Fact]
        public void ProcessName_WhenChanged_RaisesPropertyChangedEvent()
        {
            // Arrange
            var processInfo = new ProcessInfo();
            var eventRaised = false;
            processInfo.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ProcessInfo.ProcessName))
                    eventRaised = true;
            };

            // Act
            processInfo.ProcessName = "test.exe";

            // Assert
            Assert.True(eventRaised);
            Assert.Equal("test.exe", processInfo.ProcessName);
        }

        [Fact]
        public void ProcessName_WhenSetToSameValue_DoesNotRaisePropertyChangedEvent()
        {
            // Arrange
            var processInfo = new ProcessInfo { ProcessName = "test.exe" };
            var eventRaised = false;
            processInfo.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ProcessInfo.ProcessName))
                    eventRaised = true;
            };

            // Act
            processInfo.ProcessName = "test.exe";

            // Assert
            Assert.False(eventRaised);
        }

        [Fact]
        public void Id_WhenChanged_RaisesPropertyChangedEvent()
        {
            // Arrange
            var processInfo = new ProcessInfo();
            var eventRaised = false;
            processInfo.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ProcessInfo.Id))
                    eventRaised = true;
            };

            // Act
            processInfo.Id = 1234;

            // Assert
            Assert.True(eventRaised);
            Assert.Equal(1234, processInfo.Id);
        }

        [Fact]
        public void Id_WhenSetToSameValue_DoesNotRaisePropertyChangedEvent()
        {
            // Arrange
            var processInfo = new ProcessInfo { Id = 1234 };
            var eventRaised = false;
            processInfo.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ProcessInfo.Id))
                    eventRaised = true;
            };

            // Act
            processInfo.Id = 1234;

            // Assert
            Assert.False(eventRaised);
        }

        [Fact]
        public void Status_WhenChanged_RaisesPropertyChangedEvent()
        {
            // Arrange
            var processInfo = new ProcessInfo();
            var eventRaised = false;
            processInfo.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ProcessInfo.Status))
                    eventRaised = true;
            };

            // Act
            processInfo.Status = "Allowed";

            // Assert
            Assert.True(eventRaised);
            Assert.Equal("Allowed", processInfo.Status);
        }

        [Fact]
        public void Status_WhenSetToSameValue_DoesNotRaisePropertyChangedEvent()
        {
            // Arrange
            var processInfo = new ProcessInfo { Status = "Allowed" };
            var eventRaised = false;
            processInfo.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ProcessInfo.Status))
                    eventRaised = true;
            };

            // Act
            processInfo.Status = "Allowed";

            // Assert
            Assert.False(eventRaised);
        }

        [Fact]
        public void Status_WhenSetToAllowed_SetsStatusColorToGreen()
        {
            // Arrange
            var processInfo = new ProcessInfo();

            // Act
            processInfo.Status = "Allowed";

            // Assert
            Assert.Equal("Allowed", processInfo.Status);
            Assert.Equal("Green", processInfo.StatusColor);
        }

        [Fact]
        public void Status_WhenSetToBlocked_SetsStatusColorToRed()
        {
            // Arrange
            var processInfo = new ProcessInfo();

            // Act
            processInfo.Status = "Blocked";

            // Assert
            Assert.Equal("Blocked", processInfo.Status);
            Assert.Equal("Red", processInfo.StatusColor);
        }

        [Fact]
        public void Status_WhenSetToUnknownValue_SetsStatusColorToBlack()
        {
            // Arrange
            var processInfo = new ProcessInfo();

            // Act
            processInfo.Status = "Unknown";

            // Assert
            Assert.Equal("Unknown", processInfo.Status);
            Assert.Equal("Black", processInfo.StatusColor);
        }

        [Fact]
        public void Status_WhenChanged_AlsoRaisesStatusColorPropertyChangedEvent()
        {
            // Arrange
            var processInfo = new ProcessInfo();
            var statusColorEventRaised = false;
            processInfo.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ProcessInfo.StatusColor))
                    statusColorEventRaised = true;
            };

            // Act
            processInfo.Status = "Allowed";

            // Assert
            Assert.True(statusColorEventRaised);
        }

        [Fact]
        public void StatusColor_WhenChanged_RaisesPropertyChangedEvent()
        {
            // Arrange
            var processInfo = new ProcessInfo();
            var eventRaised = false;
            processInfo.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ProcessInfo.StatusColor))
                    eventRaised = true;
            };

            // Act
            processInfo.StatusColor = "Blue";

            // Assert
            Assert.True(eventRaised);
            Assert.Equal("Blue", processInfo.StatusColor);
        }

        [Fact]
        public void StatusColor_WhenSetToSameValue_DoesNotRaisePropertyChangedEvent()
        {
            // Arrange
            var processInfo = new ProcessInfo { StatusColor = "Blue" };
            var eventRaised = false;
            processInfo.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ProcessInfo.StatusColor))
                    eventRaised = true;
            };

            // Act
            processInfo.StatusColor = "Blue";

            // Assert
            Assert.False(eventRaised);
        }

        [Fact]
        public void LastSeen_WhenChanged_RaisesPropertyChangedEvent()
        {
            // Arrange
            var processInfo = new ProcessInfo();
            var eventRaised = false;
            var testTime = DateTime.Now;
            processInfo.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ProcessInfo.LastSeen))
                    eventRaised = true;
            };

            // Act
            processInfo.LastSeen = testTime;

            // Assert
            Assert.True(eventRaised);
            Assert.Equal(testTime, processInfo.LastSeen);
        }

        [Fact]
        public void LastSeen_WhenSetToSameValue_DoesNotRaisePropertyChangedEvent()
        {
            // Arrange
            var testTime = DateTime.Now;
            var processInfo = new ProcessInfo { LastSeen = testTime };
            var eventRaised = false;
            processInfo.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ProcessInfo.LastSeen))
                    eventRaised = true;
            };

            // Act
            processInfo.LastSeen = testTime;

            // Assert
            Assert.False(eventRaised);
        }

        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            // Act
            var processInfo = new ProcessInfo();

            // Assert
            Assert.Equal(string.Empty, processInfo.ProcessName);
            Assert.Equal(0, processInfo.Id);
            Assert.Equal(string.Empty, processInfo.Status);
            Assert.Equal("Black", processInfo.StatusColor);
            Assert.Equal(default(DateTime), processInfo.LastSeen);
        }

        [Theory]
        [InlineData("allowed", "Green")]
        [InlineData("ALLOWED", "Green")]
        [InlineData("Allowed", "Green")]
        [InlineData("blocked", "Red")]
        [InlineData("BLOCKED", "Red")]
        [InlineData("Blocked", "Red")]
        [InlineData("unknown", "Black")]
        [InlineData("", "Black")]
        [InlineData("invalid", "Black")]
        public void Status_CaseInsensitiveStatusColorMapping_WorksCorrectly(string status, string expectedColor)
        {
            // Arrange
            var processInfo = new ProcessInfo();

            // Act
            processInfo.Status = status;

            // Assert
            Assert.Equal(expectedColor, processInfo.StatusColor);
        }

        [Fact]
        public void PropertyChanged_WhenNoSubscribers_DoesNotThrowException()
        {
            // Arrange
            var processInfo = new ProcessInfo();

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                processInfo.ProcessName = "test.exe";
                processInfo.Id = 1234;
                processInfo.Status = "Allowed";
                processInfo.StatusColor = "Green";
                processInfo.LastSeen = DateTime.Now;
            });

            Assert.Null(exception);
        }

        [Fact]
        public void PropertyChanged_MultipleSubscribers_AllReceiveNotifications()
        {
            // Arrange
            var processInfo = new ProcessInfo();
            var subscriber1EventCount = 0;
            var subscriber2EventCount = 0;
            var subscriber1PropertyName = "";
            var subscriber2PropertyName = "";

            processInfo.PropertyChanged += (sender, e) =>
            {
                subscriber1EventCount++;
                subscriber1PropertyName = e.PropertyName ?? "";
            };

            processInfo.PropertyChanged += (sender, e) =>
            {
                subscriber2EventCount++;
                subscriber2PropertyName = e.PropertyName ?? "";
            };

            // Act
            processInfo.ProcessName = "test.exe";

            // Assert
            Assert.Equal(1, subscriber1EventCount);
            Assert.Equal(1, subscriber2EventCount);
            Assert.Equal(nameof(ProcessInfo.ProcessName), subscriber1PropertyName);
            Assert.Equal(nameof(ProcessInfo.ProcessName), subscriber2PropertyName);
        }

        [Fact]
        public void PropertyChanged_EventArgsContainCorrectSender()
        {
            // Arrange
            var processInfo = new ProcessInfo();
            object? eventSender = null;

            processInfo.PropertyChanged += (sender, e) =>
            {
                eventSender = sender;
            };

            // Act
            processInfo.ProcessName = "test.exe";

            // Assert
            Assert.Same(processInfo, eventSender);
        }

        [Fact]
        public void Status_CascadingPropertyChanges_BothEventsRaised()
        {
            // Arrange
            var processInfo = new ProcessInfo();
            var statusEventRaised = false;
            var statusColorEventRaised = false;

            processInfo.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ProcessInfo.Status))
                    statusEventRaised = true;
                if (e.PropertyName == nameof(ProcessInfo.StatusColor))
                    statusColorEventRaised = true;
            };

            // Act
            processInfo.Status = "Allowed";

            // Assert
            Assert.True(statusEventRaised);
            Assert.True(statusColorEventRaised);
        }

        [Fact]
        public void AllProperties_SequentialChanges_RaiseCorrectEvents()
        {
            // Arrange
            var processInfo = new ProcessInfo();
            var eventLog = new List<string>();

            processInfo.PropertyChanged += (sender, e) =>
            {
                eventLog.Add(e.PropertyName ?? "");
            };

            // Act
            processInfo.ProcessName = "test.exe";
            processInfo.Id = 1234;
            processInfo.Status = "Allowed";
            processInfo.LastSeen = DateTime.Now;

            // Assert
            Assert.Contains(nameof(ProcessInfo.ProcessName), eventLog);
            Assert.Contains(nameof(ProcessInfo.Id), eventLog);
            Assert.Contains(nameof(ProcessInfo.Status), eventLog);
            Assert.Contains(nameof(ProcessInfo.StatusColor), eventLog); // Should be raised by Status change
            Assert.Contains(nameof(ProcessInfo.LastSeen), eventLog);
        }

        [Fact]
        public void PropertyChanged_UnsubscribeEvent_StopsReceivingNotifications()
        {
            // Arrange
            var processInfo = new ProcessInfo();
            var eventCount = 0;

            PropertyChangedEventHandler handler = (sender, e) => eventCount++;
            processInfo.PropertyChanged += handler;

            // Act - First change should trigger event
            processInfo.ProcessName = "test1.exe";
            Assert.Equal(1, eventCount);

            // Unsubscribe
            processInfo.PropertyChanged -= handler;

            // Act - Second change should not trigger event
            processInfo.ProcessName = "test2.exe";

            // Assert
            Assert.Equal(1, eventCount); // Should still be 1
        }

        [Fact]
        public void StatusColor_DirectSet_RaisesPropertyChangedEvent()
        {
            // Arrange
            var processInfo = new ProcessInfo();
            var eventRaised = false;

            processInfo.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ProcessInfo.StatusColor))
                    eventRaised = true;
            };

            // Act
            processInfo.StatusColor = "Purple";

            // Assert
            Assert.True(eventRaised);
            Assert.Equal("Purple", processInfo.StatusColor);
        }

        [Theory]
        [InlineData("", "Black")]
        [InlineData(null, "Black")]
        [InlineData("ALLOWED", "Green")]
        [InlineData("allowed", "Green")]
        [InlineData("Allowed", "Green")]
        [InlineData("BLOCKED", "Red")]
        [InlineData("blocked", "Red")]
        [InlineData("Blocked", "Red")]
        [InlineData("Unknown", "Black")]
        [InlineData("Invalid", "Black")]
        [InlineData("Processing", "Black")]
        public void Status_VariousValues_SetsCorrectStatusColor(string? status, string expectedColor)
        {
            // Arrange
            var processInfo = new ProcessInfo();

            // Act
            processInfo.Status = status ?? "";

            // Assert
            Assert.Equal(expectedColor, processInfo.StatusColor);
        }
    }
}