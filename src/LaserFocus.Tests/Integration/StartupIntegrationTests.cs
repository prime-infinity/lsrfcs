using System;
using System.IO;
using Xunit;
using LaserFocus.Core.Services;

namespace LaserFocus.Tests.Integration
{
    /// <summary>
    /// Integration tests for application startup and configuration loading
    /// </summary>
    public class StartupIntegrationTests : IDisposable
    {
        private readonly string _testConfigDirectory;
        private readonly ConfigurationManager _configurationManager;

        public StartupIntegrationTests()
        {
            // Create a temporary directory for test configuration files
            _testConfigDirectory = Path.Combine(Path.GetTempPath(), $"LaserFocusTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testConfigDirectory);
            
            // Initialize configuration manager with test directory
            _configurationManager = new ConfigurationManager(_testConfigDirectory);
        }

        [Fact]
        public void StartupConfigurationLoading_ShouldInitializeDefaultConfiguration()
        {
            // Act
            _configurationManager.InitializeDefaultConfiguration();
            
            // Assert
            var settings = _configurationManager.LoadSettings();
            Assert.NotNull(settings);
            Assert.True(settings.MonitoringInterval > 0);
            
            var blockedWebsites = _configurationManager.LoadBlockedWebsites();
            Assert.NotNull(blockedWebsites);
            
            var allowedApps = _configurationManager.LoadAllowedApplications();
            Assert.NotNull(allowedApps);
            Assert.Contains("chrome", allowedApps);
            Assert.Contains("code", allowedApps);
            Assert.Contains("kiro", allowedApps);
        }

        [Fact]
        public void StartupConfigurationLoading_WithExistingConfig_ShouldLoadCorrectly()
        {
            // Arrange
            _configurationManager.InitializeDefaultConfiguration();
            
            // Add some test data
            var testWebsites = new List<string> { "example.com", "test.com" };
            _configurationManager.SaveBlockedWebsites(testWebsites);
            
            // Act
            var loadedWebsites = _configurationManager.LoadBlockedWebsites();
            
            // Assert
            Assert.Equal(2, loadedWebsites.Count);
            Assert.Contains("example.com", loadedWebsites);
            Assert.Contains("test.com", loadedWebsites);
        }

        [Fact]
        public void StartupConfigurationLoading_WithCorruptedConfig_ShouldHandleGracefully()
        {
            // Arrange
            var configFile = Path.Combine(_testConfigDirectory, "blocked-websites.json");
            File.WriteAllText(configFile, "invalid json content");
            
            // Act & Assert
            var exception = Record.Exception(() => _configurationManager.LoadBlockedWebsites());
            
            // Should not throw exception, should return empty list or handle gracefully
            Assert.Null(exception);
        }

        public void Dispose()
        {
            // Cleanup test directory
            if (Directory.Exists(_testConfigDirectory))
            {
                Directory.Delete(_testConfigDirectory, true);
            }
        }
    }
}