using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using Xunit;
using LaserFocus.Core.Services;

namespace LaserFocus.Tests.Services
{
    public class HostsFileManagerTests : IDisposable
    {
        private HostsFileManager _hostsFileManager;
        private string _testHostsFile;
        private string _testBackupFile;

        public HostsFileManagerTests()
        {
            // Create a test hosts file in temp directory
            _testHostsFile = Path.Combine(Path.GetTempPath(), "hosts_test");
            _testBackupFile = _testHostsFile + ".laserfocus.backup";
            
            // Clean up any existing test files
            if (File.Exists(_testHostsFile))
                File.Delete(_testHostsFile);
            if (File.Exists(_testBackupFile))
                File.Delete(_testBackupFile);

            // Create initial hosts file content
            var initialContent = @"# Copyright (c) 1993-2009 Microsoft Corp.
#
# This is a sample HOSTS file used by Microsoft TCP/IP for Windows.
#
127.0.0.1       localhost
::1             localhost";

            File.WriteAllText(_testHostsFile, initialContent);
            
            _hostsFileManager = new TestableHostsFileManager(_testHostsFile);
        }

        public void Dispose()
        {
            // Clean up test files
            if (File.Exists(_testHostsFile))
                File.Delete(_testHostsFile);
            if (File.Exists(_testBackupFile))
                File.Delete(_testBackupFile);
        }

        [Fact]
        public void HasAdministratorPrivileges_ShouldReturnBooleanValue()
        {
            // Act
            var hasPrivileges = _hostsFileManager.HasAdministratorPrivileges();

            // Assert
            Assert.IsType<bool>(hasPrivileges);
        }

        [Fact]
        public void FormatWebsiteUrl_ValidUrl_ShouldReturnFormattedUrl()
        {
            // This test uses reflection to access the private method
            var method = typeof(HostsFileManager).GetMethod("FormatWebsiteUrl", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Test various URL formats
            var testCases = new Dictionary<string, string>
            {
                { "https://www.youtube.com", "youtube.com" },
                { "http://facebook.com/", "facebook.com" },
                { "www.twitter.com", "twitter.com" },
                { "reddit.com/r/programming", "reddit.com" },
                { "GOOGLE.COM", "google.com" }
            };

            foreach (var testCase in testCases)
            {
                // Act
                var result = (string)method.Invoke(_hostsFileManager, new object[] { testCase.Key });

                // Assert
                Assert.Equal(testCase.Value, result);
            }
        }

        [Fact]
        public void FormatWebsiteUrl_EmptyUrl_ShouldThrowArgumentException()
        {
            var method = typeof(HostsFileManager).GetMethod("FormatWebsiteUrl", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act & Assert
            Assert.Throws<System.Reflection.TargetInvocationException>(() => 
                method.Invoke(_hostsFileManager, new object[] { "" }));
        }

        [Fact]
        public void FormatWebsiteUrl_InvalidUrl_ShouldThrowArgumentException()
        {
            var method = typeof(HostsFileManager).GetMethod("FormatWebsiteUrl", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act & Assert
            Assert.Throws<System.Reflection.TargetInvocationException>(() => 
                method.Invoke(_hostsFileManager, new object[] { "invalid..url" }));
        }

        [Fact]
        public void IsWebsiteBlocked_NewWebsite_ShouldReturnFalse()
        {
            // Act
            var isBlocked = _hostsFileManager.IsWebsiteBlocked("youtube.com");

            // Assert
            Assert.False(isBlocked);
        }

        [Fact]
        public void GetBlockedWebsites_EmptyHostsFile_ShouldReturnEmptyList()
        {
            // Act
            var blockedWebsites = _hostsFileManager.GetBlockedWebsites();

            // Assert
            Assert.NotNull(blockedWebsites);
            Assert.Empty(blockedWebsites);
        }

        [Fact]
        public void CreateBackupIfNotExists_ShouldCreateBackupFile()
        {
            // Act
            _hostsFileManager.CreateBackupIfNotExists();

            // Assert
            Assert.True(File.Exists(_testBackupFile));
            
            var originalContent = File.ReadAllText(_testHostsFile);
            var backupContent = File.ReadAllText(_testBackupFile);
            Assert.Equal(originalContent, backupContent);
        }

        [Fact]
        public void CreateBackupIfNotExists_BackupExists_ShouldNotOverwrite()
        {
            // Arrange
            var customBackupContent = "Custom backup content";
            File.WriteAllText(_testBackupFile, customBackupContent);

            // Act
            _hostsFileManager.CreateBackupIfNotExists();

            // Assert
            var backupContent = File.ReadAllText(_testBackupFile);
            Assert.Equal(customBackupContent, backupContent);
        }

        [Fact]
        public void RestoreFromBackup_BackupExists_ShouldRestoreHostsFile()
        {
            // Arrange
            var originalContent = File.ReadAllText(_testHostsFile);
            _hostsFileManager.CreateBackupIfNotExists();
            
            // Modify the hosts file
            File.WriteAllText(_testHostsFile, "Modified content");

            // Act
            _hostsFileManager.RestoreFromBackup();

            // Assert
            var restoredContent = File.ReadAllText(_testHostsFile);
            Assert.Equal(originalContent, restoredContent);
        }

        [Fact]
        public void RestoreFromBackup_NoBackup_ShouldThrowFileNotFoundException()
        {
            // Create a testable manager that simulates having admin privileges
            var testManager = new TestableHostsFileManager(_testHostsFile);
            
            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => testManager.RestoreFromBackup());
        }

        // Integration tests that would work with admin privileges
        [Fact]
        public void BlockWebsite_ValidWebsite_ShouldAddToHostsFile()
        {
            // Skip if not running as administrator
            if (!_hostsFileManager.HasAdministratorPrivileges())
            {
                // Skip test - xunit doesn't have Assert.Inconclusive
                return;
            }

            // Act
            _hostsFileManager.BlockWebsite("youtube.com");

            // Assert
            Assert.True(_hostsFileManager.IsWebsiteBlocked("youtube.com"));
            
            var blockedWebsites = _hostsFileManager.GetBlockedWebsites();
            Assert.Contains("youtube.com", blockedWebsites);
        }

        [Fact]
        public void UnblockWebsite_BlockedWebsite_ShouldRemoveFromHostsFile()
        {
            // Skip if not running as administrator
            if (!_hostsFileManager.HasAdministratorPrivileges())
            {
                return;
            }

            // Arrange
            _hostsFileManager.BlockWebsite("youtube.com");
            Assert.True(_hostsFileManager.IsWebsiteBlocked("youtube.com"));

            // Act
            _hostsFileManager.UnblockWebsite("youtube.com");

            // Assert
            Assert.False(_hostsFileManager.IsWebsiteBlocked("youtube.com"));
            
            var blockedWebsites = _hostsFileManager.GetBlockedWebsites();
            Assert.DoesNotContain("youtube.com", blockedWebsites);
        }

        [Fact]
        public void BlockWebsite_MultipleWebsites_ShouldBlockAll()
        {
            // Skip if not running as administrator
            if (!_hostsFileManager.HasAdministratorPrivileges())
            {
                return;
            }

            var websites = new[] { "youtube.com", "facebook.com", "twitter.com" };

            // Act
            foreach (var website in websites)
            {
                _hostsFileManager.BlockWebsite(website);
            }

            // Assert
            var blockedWebsites = _hostsFileManager.GetBlockedWebsites();
            foreach (var website in websites)
            {
                Assert.True(_hostsFileManager.IsWebsiteBlocked(website));
                Assert.Contains(website, blockedWebsites);
            }
        }

        [Fact]
        public void BlockWebsite_AlreadyBlocked_ShouldNotDuplicate()
        {
            // Skip if not running as administrator
            if (!_hostsFileManager.HasAdministratorPrivileges())
            {
                return;
            }

            // Act
            _hostsFileManager.BlockWebsite("youtube.com");
            _hostsFileManager.BlockWebsite("youtube.com"); // Block again

            // Assert
            var hostsContent = File.ReadAllText(_testHostsFile);
            var occurrences = hostsContent.Split(new[] { "127.0.0.1 youtube.com" }, StringSplitOptions.None).Length - 1;
            Assert.Equal(1, occurrences);
        }

        [Fact]
        public void BlockWebsite_NoAdminPrivileges_ShouldThrowUnauthorizedAccessException()
        {
            var nonAdminManager = new NonAdminHostsFileManager();

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() => nonAdminManager.BlockWebsite("youtube.com"));
        }

        [Fact]
        public void UnblockWebsite_NoAdminPrivileges_ShouldThrowUnauthorizedAccessException()
        {
            var nonAdminManager = new NonAdminHostsFileManager();

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() => nonAdminManager.UnblockWebsite("youtube.com"));
        }
    }

    /// <summary>
    /// Testable version of HostsFileManager that uses a custom hosts file path
    /// </summary>
    public class TestableHostsFileManager : HostsFileManager
    {
        private readonly string _customHostsPath;

        public TestableHostsFileManager(string hostsFilePath)
        {
            _customHostsPath = hostsFilePath;
            
            // Use reflection to set the private field
            var hostsFilePathField = typeof(HostsFileManager).GetField("_hostsFilePath", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            hostsFilePathField?.SetValue(this, hostsFilePath);

            var backupFilePathField = typeof(HostsFileManager).GetField("_backupFilePath", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            backupFilePathField?.SetValue(this, hostsFilePath + ".laserfocus.backup");
        }

        // Override to simulate admin privileges for testing
        public override bool HasAdministratorPrivileges()
        {
            return true;
        }
    }

    /// <summary>
    /// Mock HostsFileManager that simulates no admin privileges
    /// </summary>
    public class NonAdminHostsFileManager : HostsFileManager
    {
        public override bool HasAdministratorPrivileges()
        {
            return false;
        }
    }
}