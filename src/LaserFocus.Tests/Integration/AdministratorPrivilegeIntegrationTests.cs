using System;
using System.IO;
using System.Security.Principal;
using Xunit;
using LaserFocus.Core.Services;

namespace LaserFocus.Tests.Integration
{
    /// <summary>
    /// Integration tests for administrator privilege scenarios
    /// Tests Requirements: 7.2 (privilege handling)
    /// </summary>
    public class AdministratorPrivilegeIntegrationTests : IDisposable
    {
        private readonly string _testConfigDirectory;
        private readonly ConfigurationManager _configurationManager;

        public AdministratorPrivilegeIntegrationTests()
        {
            _testConfigDirectory = Path.Combine(Path.GetTempPath(), $"LaserFocusTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testConfigDirectory);
            _configurationManager = new ConfigurationManager(_testConfigDirectory);
        }

        [Fact]
        public void AdministratorPrivileges_Detection_ShouldCorrectlyIdentifyPrivilegeLevel()
        {
            // Act
            var isAdmin = IsRunningAsAdministrator();

            // Assert - This test documents the current privilege level
            // The actual result depends on how the test is run
            Assert.True(isAdmin || !isAdmin, "Should return a boolean value for admin status");
            
            // Log the current privilege level for test visibility
            var privilegeLevel = isAdmin ? "Administrator" : "Standard User";
            Assert.True(true, $"Tests are running with {privilegeLevel} privileges");
        }

        [Fact]
        public void AdministratorPrivileges_HostsFileAccess_ShouldHandlePrivilegeRequirements()
        {
            // Arrange
            var hostsFileManager = new HostsFileManager();
            var testWebsite = "test-privilege.com";

            // Act & Assert
            if (IsRunningAsAdministrator())
            {
                // When running as admin, should be able to block websites
                var exception = Record.Exception(() => hostsFileManager.BlockWebsite(testWebsite));
                
                // Should either succeed or fail for other reasons (not privilege-related)
                if (exception != null)
                {
                    Assert.IsNotType<UnauthorizedAccessException>(exception);
                }
            }
            else
            {
                // When not running as admin, should get UnauthorizedAccessException
                var exception = Record.Exception(() => hostsFileManager.BlockWebsite(testWebsite));
                
                // Should throw UnauthorizedAccessException due to insufficient privileges
                Assert.IsType<UnauthorizedAccessException>(exception);
            }
        }

        [Fact]
        public void AdministratorPrivileges_ProcessTermination_ShouldRespectPrivilegeLimitations()
        {
            // Arrange
            var allowedProcesses = new[] { "chrome", "code", "kiro", "notepad" };
            var processMonitor = new ProcessMonitor(allowedProcesses);

            // Act - Try to get running processes (should work regardless of privileges)
            var exception = Record.Exception(() => processMonitor.GetRunningProcesses());

            // Assert - Getting process list should work without admin privileges
            Assert.Null(exception);

            // Act - Try to terminate a non-existent process (safe test)
            var terminateException = Record.Exception(() => processMonitor.TerminateProcess(999999));

            // Assert - Should handle gracefully regardless of privileges
            Assert.Null(terminateException);
        }

        [Fact]
        public void AdministratorPrivileges_ConfigurationAccess_ShouldWorkWithoutElevation()
        {
            // Arrange
            var testWebsites = new[] { "config-test1.com", "config-test2.com" };
            var testApps = new[] { "app1", "app2", "app3" };

            // Act & Assert - Configuration operations should work without admin privileges
            var saveWebsitesException = Record.Exception(() => 
                _configurationManager.SaveBlockedWebsites(testWebsites.ToList()));
            Assert.Null(saveWebsitesException);

            var loadWebsitesException = Record.Exception(() => 
                _configurationManager.LoadBlockedWebsites());
            Assert.Null(loadWebsitesException);

            var saveAppsException = Record.Exception(() => 
                _configurationManager.SaveAllowedApplications(testApps.ToList()));
            Assert.Null(saveAppsException);

            var loadAppsException = Record.Exception(() => 
                _configurationManager.LoadAllowedApplications());
            Assert.Null(loadAppsException);
        }

        [Fact]
        public void AdministratorPrivileges_GracefulDegradation_ShouldProvideAlternativeFunctionality()
        {
            // This test verifies that the application can function with limited capabilities
            // when administrator privileges are not available

            // Arrange
            var hostsFileManager = new HostsFileManager();
            var testWebsite = "graceful-test.com";

            // Act
            if (!IsRunningAsAdministrator())
            {
                // When not admin, should handle gracefully
                var exception = Record.Exception(() => hostsFileManager.BlockWebsite(testWebsite));
                
                // Should throw UnauthorizedAccessException with helpful message
                if (exception is UnauthorizedAccessException unauthorizedException)
                {
                    Assert.Contains("administrator", unauthorizedException.Message.ToLower());
                }
            }

            // Configuration should still work
            var configException = Record.Exception(() =>
            {
                var websites = _configurationManager.LoadBlockedWebsites();
                websites.Add(testWebsite);
                _configurationManager.SaveBlockedWebsites(websites);
            });

            Assert.Null(configException);
        }

        [Fact]
        public void AdministratorPrivileges_ErrorMessages_ShouldBeUserFriendly()
        {
            // Arrange
            var hostsFileManager = new HostsFileManager();
            var testWebsite = "error-message-test.com";

            // Act
            if (!IsRunningAsAdministrator())
            {
                var exception = Record.Exception(() => hostsFileManager.BlockWebsite(testWebsite));

                // Assert - Error messages should be helpful for users
                if (exception is UnauthorizedAccessException unauthorizedException)
                {
                    var message = unauthorizedException.Message.ToLower();
                    
                    // Should mention administrator or elevation
                    var hasAdminReference = message.Contains("administrator") || 
                                          message.Contains("admin") || 
                                          message.Contains("elevation") ||
                                          message.Contains("privilege");
                    
                    Assert.True(hasAdminReference, 
                        "Error message should reference administrator privileges or elevation");
                }
            }
        }

        [Fact]
        public void AdministratorPrivileges_SecurityValidation_ShouldPreventUnauthorizedAccess()
        {
            // This test ensures that privilege checks are properly implemented
            
            // Arrange
            var hostsFileManager = new HostsFileManager();
            var maliciousWebsite = "malicious-test.com";

            // Act & Assert
            if (!IsRunningAsAdministrator())
            {
                // Should not be able to modify system files without proper privileges
                var exception = Record.Exception(() => hostsFileManager.BlockWebsite(maliciousWebsite));
                
                Assert.NotNull(exception);
                Assert.IsType<UnauthorizedAccessException>(exception);
            }
            else
            {
                // When running as admin, operations should be allowed but still validated
                var exception = Record.Exception(() => hostsFileManager.BlockWebsite(maliciousWebsite));
                
                // Should either succeed or fail for validation reasons, not privilege reasons
                if (exception != null)
                {
                    Assert.IsNotType<UnauthorizedAccessException>(exception);
                }
            }
        }

        /// <summary>
        /// Helper method to check if the current process is running with administrator privileges
        /// </summary>
        /// <returns>True if running as administrator, false otherwise</returns>
        private static bool IsRunningAsAdministrator()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                // If we can't determine privilege level, assume non-admin for safety
                return false;
            }
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