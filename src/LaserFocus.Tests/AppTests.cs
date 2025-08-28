using System;
using Xunit;
using LaserFocus;

namespace LaserFocus.Tests
{
    /// <summary>
    /// Tests for the App class startup and privilege handling functionality
    /// </summary>
    public class AppTests
    {
        [Fact]
        public void HasAdministratorPrivileges_WhenNotInitialized_ShouldReturnFalse()
        {
            // Arrange & Act
            // Note: This test assumes no App.Current is running
            bool hasPrivileges = false;
            
            try
            {
                hasPrivileges = App.HasAdministratorPrivileges();
            }
            catch (Exception)
            {
                // Expected when no current application instance
                hasPrivileges = false;
            }
            
            // Assert
            Assert.False(hasPrivileges);
        }

        [Fact]
        public void GetConfigurationManager_WhenNotInitialized_ShouldReturnNull()
        {
            // Arrange & Act
            var configManager = App.GetConfigurationManager();
            
            // Assert
            // Should return null when no application instance is running
            Assert.Null(configManager);
        }

        [Fact]
        public void App_StaticMethods_ShouldHandleNullCurrentGracefully()
        {
            // Arrange & Act & Assert
            // These methods should not throw exceptions even when App.Current is null
            
            // Test HasAdministratorPrivileges
            var hasPrivileges = false;
            var exception1 = Record.Exception(() => hasPrivileges = App.HasAdministratorPrivileges());
            
            // Test GetConfigurationManager
            var configManager = default(object);
            var exception2 = Record.Exception(() => configManager = App.GetConfigurationManager());
            
            // Assert no exceptions were thrown
            Assert.Null(exception1);
            Assert.Null(exception2);
            
            // Assert expected default values
            Assert.False(hasPrivileges);
            Assert.Null(configManager);
        }
    }
}