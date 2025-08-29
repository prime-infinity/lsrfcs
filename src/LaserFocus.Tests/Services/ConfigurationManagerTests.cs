using LaserFocus.Core.Models;
using LaserFocus.Core.Services;
using Xunit;

namespace LaserFocus.Tests.Services
{
    public class ConfigurationManagerTests : IDisposable
    {
        private readonly string _testConfigDirectory;
        private readonly ConfigurationManager _configManager;

        public ConfigurationManagerTests()
        {
            // Create a temporary directory for test configuration files
            _testConfigDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(_testConfigDirectory);
            _configManager = new ConfigurationManager(_testConfigDirectory);
        }

        public void Dispose()
        {
            // Clean up test directory
            if (System.IO.Directory.Exists(_testConfigDirectory))
            {
                System.IO.Directory.Delete(_testConfigDirectory, true);
            }
        }

        [Fact]
        public void LoadBlockedWebsites_WhenFileDoesNotExist_ReturnsDefaultList()
        {
            // Act
            var websites = _configManager.LoadBlockedWebsites();

            // Assert
            Assert.NotNull(websites);
            Assert.True(websites.Count > 0);
            Assert.Contains("youtube.com", websites);
            Assert.Contains("facebook.com", websites);
        }

        [Fact]
        public void SaveAndLoadBlockedWebsites_WhenValidData_PersistsCorrectly()
        {
            // Arrange
            var testWebsites = new List<string> { "example.com", "test.com", "blocked.org" };

            // Act
            _configManager.SaveBlockedWebsites(testWebsites);
            var loadedWebsites = _configManager.LoadBlockedWebsites();

            // Assert
            Assert.NotNull(loadedWebsites);
            Assert.Equal(testWebsites.Count, loadedWebsites.Count);
            Assert.Equal(testWebsites, loadedWebsites);
        }

        [Fact]
        public void LoadAllowedApplications_WhenFileDoesNotExist_ReturnsDefaultList()
        {
            // Act
            var applications = _configManager.LoadAllowedApplications();

            // Assert
            Assert.NotNull(applications);
            Assert.True(applications.Count > 0);
            Assert.Contains("chrome", applications);
            Assert.Contains("code", applications);
            Assert.Contains("kiro", applications);
        }

        [Fact]
        public void SaveAndLoadAllowedApplications_WhenValidData_PersistsCorrectly()
        {
            // Arrange
            var testApplications = new List<string> { "notepad", "calculator", "paint" };

            // Act
            _configManager.SaveAllowedApplications(testApplications);
            var loadedApplications = _configManager.LoadAllowedApplications();

            // Assert
            Assert.NotNull(loadedApplications);
            Assert.Equal(testApplications.Count, loadedApplications.Count);
            Assert.Equal(testApplications, loadedApplications);
        }

        [Fact]
        public void LoadSettings_WhenFileDoesNotExist_ReturnsDefaultSettings()
        {
            // Act
            var settings = _configManager.LoadSettings();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal(2000, settings.MonitoringInterval);
            Assert.False(settings.StartMinimized);
            Assert.True(settings.ShowNotifications);
            Assert.NotNull(settings.BlockedWebsites);
            Assert.NotNull(settings.AllowedApplications);
        }

        [Fact]
        public void SaveAndLoadSettings_WhenValidData_PersistsCorrectly()
        {
            // Arrange
            var testSettings = new AppSettings
            {
                MonitoringInterval = 5000,
                StartMinimized = true,
                ShowNotifications = false,
                BlockedWebsites = new List<string> { "test.com" },
                AllowedApplications = new List<string> { "testapp" }
            };

            // Act
            _configManager.SaveSettings(testSettings);
            var loadedSettings = _configManager.LoadSettings();

            // Assert
            Assert.NotNull(loadedSettings);
            Assert.Equal(testSettings.MonitoringInterval, loadedSettings.MonitoringInterval);
            Assert.Equal(testSettings.StartMinimized, loadedSettings.StartMinimized);
            Assert.Equal(testSettings.ShowNotifications, loadedSettings.ShowNotifications);
            Assert.Equal(testSettings.BlockedWebsites, loadedSettings.BlockedWebsites);
            Assert.Equal(testSettings.AllowedApplications, loadedSettings.AllowedApplications);
        }

        [Fact]
        public void InitializeDefaultConfiguration_WhenCalled_CreatesAllConfigFiles()
        {
            // Act
            _configManager.InitializeDefaultConfiguration();

            // Assert
            var blockedWebsitesPath = System.IO.Path.Combine(_testConfigDirectory, "blocked-websites.json");
            var allowedApplicationsPath = System.IO.Path.Combine(_testConfigDirectory, "allowed-applications.json");
            var appSettingsPath = System.IO.Path.Combine(_testConfigDirectory, "app-settings.json");

            Assert.True(System.IO.File.Exists(blockedWebsitesPath));
            Assert.True(System.IO.File.Exists(allowedApplicationsPath));
            Assert.True(System.IO.File.Exists(appSettingsPath));
        }

        [Fact]
        public void SaveBlockedWebsites_WhenEmptyList_SavesEmptyArray()
        {
            // Arrange
            var emptyList = new List<string>();

            // Act
            _configManager.SaveBlockedWebsites(emptyList);
            var loadedWebsites = _configManager.LoadBlockedWebsites();

            // Assert
            Assert.NotNull(loadedWebsites);
            Assert.Empty(loadedWebsites);
        }

        [Fact]
        public void SaveAllowedApplications_WhenEmptyList_SavesEmptyArray()
        {
            // Arrange
            var emptyList = new List<string>();

            // Act
            _configManager.SaveAllowedApplications(emptyList);
            var loadedApplications = _configManager.LoadAllowedApplications();

            // Assert
            Assert.NotNull(loadedApplications);
            Assert.Empty(loadedApplications);
        }

        [Fact]
        public void Constructor_WhenInvalidPath_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new ConfigurationManager("Z:\\InvalidPath\\DoesNotExist"));
        }

        [Fact]
        public void SaveBlockedWebsites_WhenNullList_SavesEmptyList()
        {
            // Act
            _configManager.SaveBlockedWebsites(null!);
            var loadedWebsites = _configManager.LoadBlockedWebsites();

            // Assert
            Assert.NotNull(loadedWebsites);
            Assert.Empty(loadedWebsites);
        }

        [Fact]
        public void LoadBlockedWebsites_WhenCorruptedJson_ReturnsDefaultList()
        {
            // Arrange
            var corruptedJson = "{ invalid json content }";
            var blockedWebsitesPath = System.IO.Path.Combine(_testConfigDirectory, "blocked-websites.json");
            System.IO.File.WriteAllText(blockedWebsitesPath, corruptedJson);

            // Act
            var websites = _configManager.LoadBlockedWebsites();

            // Assert
            Assert.NotNull(websites);
            Assert.True(websites.Count > 0); // Should return default list
            Assert.Contains("youtube.com", websites);
        }

        [Fact]
        public void LoadAllowedApplications_WhenCorruptedJson_ReturnsDefaultList()
        {
            // Arrange
            var corruptedJson = "{ invalid json content }";
            var allowedAppsPath = System.IO.Path.Combine(_testConfigDirectory, "allowed-applications.json");
            System.IO.File.WriteAllText(allowedAppsPath, corruptedJson);

            // Act
            var applications = _configManager.LoadAllowedApplications();

            // Assert
            Assert.NotNull(applications);
            Assert.True(applications.Count > 0); // Should return default list
            Assert.Contains("chrome", applications);
        }

        [Fact]
        public void LoadSettings_WhenCorruptedJson_ReturnsDefaultSettings()
        {
            // Arrange
            var corruptedJson = "{ invalid json content }";
            var settingsPath = System.IO.Path.Combine(_testConfigDirectory, "app-settings.json");
            System.IO.File.WriteAllText(settingsPath, corruptedJson);

            // Act
            var settings = _configManager.LoadSettings();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal(2000, settings.MonitoringInterval); // Default value
            Assert.False(settings.StartMinimized); // Default value
            Assert.True(settings.ShowNotifications); // Default value
        }

        [Fact]
        public void SaveBlockedWebsites_WhenDirectoryDoesNotExist_CreatesDirectory()
        {
            // Arrange
            var newConfigDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
            var newConfigManager = new ConfigurationManager(newConfigDir);
            var testWebsites = new List<string> { "test.com" };

            try
            {
                // Act
                newConfigManager.SaveBlockedWebsites(testWebsites);

                // Assert
                Assert.True(System.IO.Directory.Exists(newConfigDir));
                var savedWebsites = newConfigManager.LoadBlockedWebsites();
                Assert.Equal(testWebsites, savedWebsites);
            }
            finally
            {
                // Cleanup
                if (System.IO.Directory.Exists(newConfigDir))
                {
                    System.IO.Directory.Delete(newConfigDir, true);
                }
            }
        }

        [Fact]
        public void SaveAllowedApplications_WhenDirectoryDoesNotExist_CreatesDirectory()
        {
            // Arrange
            var newConfigDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
            var newConfigManager = new ConfigurationManager(newConfigDir);
            var testApps = new List<string> { "testapp" };

            try
            {
                // Act
                newConfigManager.SaveAllowedApplications(testApps);

                // Assert
                Assert.True(System.IO.Directory.Exists(newConfigDir));
                var savedApps = newConfigManager.LoadAllowedApplications();
                Assert.Equal(testApps, savedApps);
            }
            finally
            {
                // Cleanup
                if (System.IO.Directory.Exists(newConfigDir))
                {
                    System.IO.Directory.Delete(newConfigDir, true);
                }
            }
        }

        [Fact]
        public void SaveSettings_WhenDirectoryDoesNotExist_CreatesDirectory()
        {
            // Arrange
            var newConfigDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
            var newConfigManager = new ConfigurationManager(newConfigDir);
            var testSettings = new AppSettings { MonitoringInterval = 5000 };

            try
            {
                // Act
                newConfigManager.SaveSettings(testSettings);

                // Assert
                Assert.True(System.IO.Directory.Exists(newConfigDir));
                var savedSettings = newConfigManager.LoadSettings();
                Assert.Equal(testSettings.MonitoringInterval, savedSettings.MonitoringInterval);
            }
            finally
            {
                // Cleanup
                if (System.IO.Directory.Exists(newConfigDir))
                {
                    System.IO.Directory.Delete(newConfigDir, true);
                }
            }
        }

        [Fact]
        public void LoadBlockedWebsites_WhenEmptyJsonFile_ReturnsDefaultList()
        {
            // Arrange
            var blockedWebsitesPath = System.IO.Path.Combine(_testConfigDirectory, "blocked-websites.json");
            System.IO.File.WriteAllText(blockedWebsitesPath, "");

            // Act
            var websites = _configManager.LoadBlockedWebsites();

            // Assert
            Assert.NotNull(websites);
            Assert.True(websites.Count > 0); // Should return default list
        }

        [Fact]
        public void LoadBlockedWebsites_WhenWhitespaceOnlyFile_ReturnsDefaultList()
        {
            // Arrange
            var blockedWebsitesPath = System.IO.Path.Combine(_testConfigDirectory, "blocked-websites.json");
            System.IO.File.WriteAllText(blockedWebsitesPath, "   \n\t  ");

            // Act
            var websites = _configManager.LoadBlockedWebsites();

            // Assert
            Assert.NotNull(websites);
            Assert.True(websites.Count > 0); // Should return default list
        }

        [Fact]
        public void SaveAndLoadComplexSettings_WithAllProperties_PersistsCorrectly()
        {
            // Arrange
            var complexSettings = new AppSettings
            {
                MonitoringInterval = 1500,
                StartMinimized = true,
                ShowNotifications = false,
                BlockedWebsites = new List<string> 
                { 
                    "youtube.com", 
                    "facebook.com", 
                    "twitter.com",
                    "reddit.com",
                    "instagram.com"
                },
                AllowedApplications = new List<string> 
                { 
                    "chrome", 
                    "firefox", 
                    "code", 
                    "notepad++", 
                    "kiro" 
                }
            };

            // Act
            _configManager.SaveSettings(complexSettings);
            var loadedSettings = _configManager.LoadSettings();

            // Assert
            Assert.Equal(complexSettings.MonitoringInterval, loadedSettings.MonitoringInterval);
            Assert.Equal(complexSettings.StartMinimized, loadedSettings.StartMinimized);
            Assert.Equal(complexSettings.ShowNotifications, loadedSettings.ShowNotifications);
            Assert.Equal(complexSettings.BlockedWebsites.Count, loadedSettings.BlockedWebsites.Count);
            Assert.Equal(complexSettings.AllowedApplications.Count, loadedSettings.AllowedApplications.Count);
            
            foreach (var website in complexSettings.BlockedWebsites)
            {
                Assert.Contains(website, loadedSettings.BlockedWebsites);
            }
            
            foreach (var app in complexSettings.AllowedApplications)
            {
                Assert.Contains(app, loadedSettings.AllowedApplications);
            }
        }
    }
}