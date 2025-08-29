using LaserFocus.Core.Models;
using Xunit;

namespace LaserFocus.Tests.Models
{
    public class AppSettingsTests
    {
        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            // Act
            var appSettings = new AppSettings();

            // Assert
            Assert.NotNull(appSettings.BlockedWebsites);
            Assert.Empty(appSettings.BlockedWebsites);
            
            Assert.NotNull(appSettings.AllowedApplications);
            Assert.Contains("chrome", appSettings.AllowedApplications);
            Assert.Contains("code", appSettings.AllowedApplications);
            Assert.Contains("kiro", appSettings.AllowedApplications);
            
            Assert.Equal(2000, appSettings.MonitoringInterval);
            Assert.False(appSettings.StartMinimized);
            Assert.True(appSettings.ShowNotifications);
        }

        [Fact]
        public void BlockedWebsites_CanBeModified()
        {
            // Arrange
            var appSettings = new AppSettings();
            var testWebsites = new List<string> { "youtube.com", "facebook.com" };

            // Act
            appSettings.BlockedWebsites = testWebsites;

            // Assert
            Assert.Equal(testWebsites, appSettings.BlockedWebsites);
            Assert.Equal(2, appSettings.BlockedWebsites.Count);
        }

        [Fact]
        public void AllowedApplications_CanBeModified()
        {
            // Arrange
            var appSettings = new AppSettings();
            var testApplications = new List<string> { "notepad", "calculator" };

            // Act
            appSettings.AllowedApplications = testApplications;

            // Assert
            Assert.Equal(testApplications, appSettings.AllowedApplications);
            Assert.Equal(2, appSettings.AllowedApplications.Count);
        }

        [Fact]
        public void MonitoringInterval_CanBeSet()
        {
            // Arrange
            var appSettings = new AppSettings();
            var testInterval = 5000;

            // Act
            appSettings.MonitoringInterval = testInterval;

            // Assert
            Assert.Equal(testInterval, appSettings.MonitoringInterval);
        }

        [Fact]
        public void StartMinimized_CanBeSet()
        {
            // Arrange
            var appSettings = new AppSettings();

            // Act
            appSettings.StartMinimized = true;

            // Assert
            Assert.True(appSettings.StartMinimized);
        }

        [Fact]
        public void ShowNotifications_CanBeSet()
        {
            // Arrange
            var appSettings = new AppSettings();

            // Act
            appSettings.ShowNotifications = false;

            // Assert
            Assert.False(appSettings.ShowNotifications);
        }

        [Fact]
        public void AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var appSettings = new AppSettings();
            var blockedWebsites = new List<string> { "test.com" };
            var allowedApplications = new List<string> { "testapp" };
            var monitoringInterval = 3000;
            var startMinimized = true;
            var showNotifications = false;

            // Act
            appSettings.BlockedWebsites = blockedWebsites;
            appSettings.AllowedApplications = allowedApplications;
            appSettings.MonitoringInterval = monitoringInterval;
            appSettings.StartMinimized = startMinimized;
            appSettings.ShowNotifications = showNotifications;

            // Assert
            Assert.Equal(blockedWebsites, appSettings.BlockedWebsites);
            Assert.Equal(allowedApplications, appSettings.AllowedApplications);
            Assert.Equal(monitoringInterval, appSettings.MonitoringInterval);
            Assert.Equal(startMinimized, appSettings.StartMinimized);
            Assert.Equal(showNotifications, appSettings.ShowNotifications);
        }

        [Fact]
        public void DefaultAllowedApplications_ContainsExpectedApps()
        {
            // Act
            var appSettings = new AppSettings();

            // Assert
            Assert.Equal(3, appSettings.AllowedApplications.Count);
            Assert.Contains("chrome", appSettings.AllowedApplications);
            Assert.Contains("code", appSettings.AllowedApplications);
            Assert.Contains("kiro", appSettings.AllowedApplications);
        }

        [Fact]
        public void MonitoringInterval_DefaultValue_IsReasonable()
        {
            // Act
            var appSettings = new AppSettings();

            // Assert
            Assert.True(appSettings.MonitoringInterval > 0);
            Assert.True(appSettings.MonitoringInterval <= 10000); // Should be reasonable for UI updates
        }
    }
}