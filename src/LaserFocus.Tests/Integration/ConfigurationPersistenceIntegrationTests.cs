using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using LaserFocus.Core.Models;
using LaserFocus.Core.Services;

namespace LaserFocus.Tests.Integration
{
    /// <summary>
    /// Integration tests for configuration persistence across application restarts
    /// Tests Requirements: 4.1, 7.2
    /// </summary>
    public class ConfigurationPersistenceIntegrationTests : IDisposable
    {
        private readonly string _testConfigDirectory;
        private readonly List<string> _testDirectories;

        public ConfigurationPersistenceIntegrationTests()
        {
            _testDirectories = new List<string>();
            _testConfigDirectory = CreateTestDirectory();
        }

        private string CreateTestDirectory()
        {
            var directory = Path.Combine(Path.GetTempPath(), $"LaserFocusTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(directory);
            _testDirectories.Add(directory);
            return directory;
        }

        [Fact]
        public void ConfigurationPersistence_BlockedWebsites_ShouldSurviveMultipleRestarts()
        {
            // Arrange
            var testWebsites = new List<string> 
            { 
                "youtube.com", 
                "facebook.com", 
                "twitter.com", 
                "reddit.com" 
            };

            // Act - First application session
            var configManager1 = new ConfigurationManager(_testConfigDirectory);
            configManager1.SaveBlockedWebsites(testWebsites);

            // Simulate first restart
            var configManager2 = new ConfigurationManager(_testConfigDirectory);
            var loadedWebsites1 = configManager2.LoadBlockedWebsites();

            // Modify configuration
            loadedWebsites1.Add("instagram.com");
            loadedWebsites1.Remove("twitter.com");
            configManager2.SaveBlockedWebsites(loadedWebsites1);

            // Simulate second restart
            var configManager3 = new ConfigurationManager(_testConfigDirectory);
            var loadedWebsites2 = configManager3.LoadBlockedWebsites();

            // Assert
            Assert.Contains("youtube.com", loadedWebsites2);
            Assert.Contains("facebook.com", loadedWebsites2);
            Assert.Contains("reddit.com", loadedWebsites2);
            Assert.Contains("instagram.com", loadedWebsites2);
            Assert.DoesNotContain("twitter.com", loadedWebsites2);
        }

        [Fact]
        public void ConfigurationPersistence_AllowedApplications_ShouldMaintainConsistency()
        {
            // Arrange
            var initialApps = new List<string> { "chrome", "code", "kiro" };
            var additionalApps = new List<string> { "devenv", "notepad", "calc" };

            // Act - First session: Save initial configuration
            var configManager1 = new ConfigurationManager(_testConfigDirectory);
            configManager1.SaveAllowedApplications(initialApps);

            // Second session: Load and modify
            var configManager2 = new ConfigurationManager(_testConfigDirectory);
            var loadedApps = configManager2.LoadAllowedApplications();
            loadedApps.AddRange(additionalApps);
            configManager2.SaveAllowedApplications(loadedApps);

            // Third session: Verify persistence
            var configManager3 = new ConfigurationManager(_testConfigDirectory);
            var finalApps = configManager3.LoadAllowedApplications();

            // Assert
            var expectedApps = initialApps.Concat(additionalApps).ToList();
            Assert.Equal(expectedApps.Count, finalApps.Count);
            
            foreach (var app in expectedApps)
            {
                Assert.Contains(app, finalApps);
            }
        }

        [Fact]
        public void ConfigurationPersistence_AppSettings_ShouldPreserveAllProperties()
        {
            // Arrange
            var originalSettings = new AppSettings
            {
                MonitoringInterval = 3000,
                StartMinimized = true,
                ShowNotifications = false,
                BlockedWebsites = new List<string> { "test1.com", "test2.com" },
                AllowedApplications = new List<string> { "app1", "app2" }
            };

            // Act - Save settings
            var configManager1 = new ConfigurationManager(_testConfigDirectory);
            configManager1.SaveSettings(originalSettings);

            // Simulate restart and load
            var configManager2 = new ConfigurationManager(_testConfigDirectory);
            var loadedSettings = configManager2.LoadSettings();

            // Assert
            Assert.NotNull(loadedSettings);
            Assert.Equal(originalSettings.MonitoringInterval, loadedSettings.MonitoringInterval);
            Assert.Equal(originalSettings.StartMinimized, loadedSettings.StartMinimized);
            Assert.Equal(originalSettings.ShowNotifications, loadedSettings.ShowNotifications);
            
            // Verify collections
            Assert.Equal(originalSettings.BlockedWebsites.Count, loadedSettings.BlockedWebsites.Count);
            Assert.Equal(originalSettings.AllowedApplications.Count, loadedSettings.AllowedApplications.Count);
            
            foreach (var website in originalSettings.BlockedWebsites)
            {
                Assert.Contains(website, loadedSettings.BlockedWebsites);
            }
            
            foreach (var app in originalSettings.AllowedApplications)
            {
                Assert.Contains(app, loadedSettings.AllowedApplications);
            }
        }

        [Fact]
        public void ConfigurationPersistence_CorruptedFiles_ShouldRecoverGracefully()
        {
            // Arrange - Create corrupted configuration files
            var configManager = new ConfigurationManager(_testConfigDirectory);
            
            var blockedWebsitesPath = Path.Combine(_testConfigDirectory, "blocked-websites.json");
            var allowedAppsPath = Path.Combine(_testConfigDirectory, "allowed-applications.json");
            var settingsPath = Path.Combine(_testConfigDirectory, "app-settings.json");

            // Create corrupted JSON files
            File.WriteAllText(blockedWebsitesPath, "{ invalid json content");
            File.WriteAllText(allowedAppsPath, "[ incomplete array");
            File.WriteAllText(settingsPath, "not json at all");

            // Act & Assert - Should handle corruption gracefully
            var websites = configManager.LoadBlockedWebsites();
            var apps = configManager.LoadAllowedApplications();
            var settings = configManager.LoadSettings();

            // Should return default values instead of throwing exceptions
            Assert.NotNull(websites);
            Assert.NotNull(apps);
            Assert.NotNull(settings);
        }

        [Fact]
        public void ConfigurationPersistence_ConcurrentAccess_ShouldHandleMultipleInstances()
        {
            // Arrange
            var testWebsites1 = new List<string> { "site1.com", "site2.com" };
            var testWebsites2 = new List<string> { "site3.com", "site4.com" };

            // Act - Simulate concurrent access from multiple instances
            var configManager1 = new ConfigurationManager(_testConfigDirectory);
            var configManager2 = new ConfigurationManager(_testConfigDirectory);

            // Both instances save different data
            configManager1.SaveBlockedWebsites(testWebsites1);
            configManager2.SaveBlockedWebsites(testWebsites2);

            // Load from a third instance
            var configManager3 = new ConfigurationManager(_testConfigDirectory);
            var loadedWebsites = configManager3.LoadBlockedWebsites();

            // Assert - Should have the last saved data
            Assert.NotNull(loadedWebsites);
            // The exact content depends on timing, but it should be one of the saved sets
            var isFirstSet = testWebsites1.All(w => loadedWebsites.Contains(w)) && 
                           loadedWebsites.Count == testWebsites1.Count;
            var isSecondSet = testWebsites2.All(w => loadedWebsites.Contains(w)) && 
                            loadedWebsites.Count == testWebsites2.Count;
            
            Assert.True(isFirstSet || isSecondSet, "Should contain one of the saved website sets");
        }

        [Fact]
        public void ConfigurationPersistence_DefaultInitialization_ShouldCreateValidDefaults()
        {
            // Arrange - Use a fresh directory
            var freshDirectory = CreateTestDirectory();

            // Act - Initialize configuration manager with empty directory
            var configManager = new ConfigurationManager(freshDirectory);
            configManager.InitializeDefaultConfiguration();

            // Load all configurations
            var websites = configManager.LoadBlockedWebsites();
            var apps = configManager.LoadAllowedApplications();
            var settings = configManager.LoadSettings();

            // Assert
            Assert.NotNull(websites);
            Assert.NotNull(apps);
            Assert.NotNull(settings);

            // Verify default allowed applications include essential development tools
            Assert.Contains("chrome", apps);
            Assert.Contains("code", apps);
            Assert.Contains("kiro", apps);

            // Verify settings have reasonable defaults
            Assert.True(settings.MonitoringInterval > 0);
            Assert.True(settings.MonitoringInterval <= 10000); // Reasonable upper bound
        }

        public void Dispose()
        {
            // Cleanup all test directories
            foreach (var directory in _testDirectories)
            {
                try
                {
                    if (Directory.Exists(directory))
                    {
                        Directory.Delete(directory, true);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}