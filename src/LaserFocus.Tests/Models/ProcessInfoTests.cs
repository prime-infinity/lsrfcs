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
    }
}