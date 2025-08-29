using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Xunit;
using LaserFocus.Core.Models;
using LaserFocus.Core.Services;

namespace LaserFocus.Tests.Integration
{
    /// <summary>
    /// Integration tests for UI data binding and real-time updates
    /// Tests Requirements: 4.1 (real-time process list), 7.2 (UI integration)
    /// </summary>
    public class UIDataBindingIntegrationTests : IDisposable
    {
        private readonly string _testConfigDirectory;
        private readonly ConfigurationManager _configurationManager;
        private readonly ProcessMonitor _processMonitor;

        public UIDataBindingIntegrationTests()
        {
            _testConfigDirectory = Path.Combine(Path.GetTempPath(), $"LaserFocusTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testConfigDirectory);
            
            _configurationManager = new ConfigurationManager(_testConfigDirectory);
            _processMonitor = new ProcessMonitor(new[] { "chrome", "code", "kiro", "notepad" });
        }

        [Fact]
        public void UIDataBinding_ObservableCollection_ShouldNotifyPropertyChanges()
        {
            // Arrange
            var blockedWebsites = new ObservableCollection<string>();
            var propertyChangedEvents = new List<string>();

            // Subscribe to property change notifications
            ((INotifyPropertyChanged)blockedWebsites).PropertyChanged += (sender, e) =>
            {
                propertyChangedEvents.Add(e.PropertyName ?? string.Empty);
            };

            // Act - Modify collection
            blockedWebsites.Add("youtube.com");
            blockedWebsites.Add("facebook.com");
            blockedWebsites.Remove("youtube.com");
            blockedWebsites.Clear();

            // Assert - Should have received property change notifications
            Assert.True(propertyChangedEvents.Count > 0, "Should receive property change notifications");
            
            // ObservableCollection typically fires "Count" and "Item[]" property changes
            Assert.Contains("Count", propertyChangedEvents);
            Assert.Contains("Item[]", propertyChangedEvents);
        }

        [Fact]
        public void UIDataBinding_ProcessInfo_ShouldImplementINotifyPropertyChanged()
        {
            // Arrange
            var processInfo = new ProcessInfo
            {
                ProcessName = "test",
                Id = 1234,
                Status = "Allowed"
            };

            var propertyChangedEvents = new List<string>();
            processInfo.PropertyChanged += (sender, e) =>
            {
                propertyChangedEvents.Add(e.PropertyName ?? string.Empty);
            };

            // Act - Change properties
            processInfo.Status = "Blocked";
            processInfo.ProcessName = "updated";

            // Assert
            Assert.Contains("Status", propertyChangedEvents);
            Assert.Contains("ProcessName", propertyChangedEvents);
        }

        [Fact]
        public void UIDataBinding_ProcessCollection_ShouldUpdateWithRealTimeData()
        {
            // Arrange
            var runningProcesses = new ObservableCollection<ProcessInfo>();
            var collectionChangedCount = 0;

            runningProcesses.CollectionChanged += (sender, e) =>
            {
                collectionChangedCount++;
            };

            // Act - Simulate real-time process updates
            var currentProcesses = _processMonitor.GetRunningProcesses();
            
            // Add processes to collection
            foreach (var process in currentProcesses.Take(5)) // Limit for test performance
            {
                runningProcesses.Add(process);
            }

            // Simulate process status changes
            if (runningProcesses.Count > 0)
            {
                var firstProcess = runningProcesses[0];
                var originalStatus = firstProcess.Status;
                firstProcess.Status = originalStatus == "Allowed" ? "Blocked" : "Allowed";
            }

            // Assert
            Assert.True(collectionChangedCount > 0, "Collection should notify of changes");
            Assert.True(runningProcesses.Count > 0, "Should have added processes to collection");
        }

        [Fact]
        public void UIDataBinding_ConfigurationChanges_ShouldReflectInCollections()
        {
            // Arrange
            var blockedWebsites = new ObservableCollection<string>();
            var initialWebsites = new List<string> { "site1.com", "site2.com" };

            // Load initial configuration
            _configurationManager.SaveBlockedWebsites(initialWebsites);
            var loadedWebsites = _configurationManager.LoadBlockedWebsites();

            // Populate observable collection
            foreach (var website in loadedWebsites)
            {
                blockedWebsites.Add(website);
            }

            // Act - Modify configuration and update collection
            var newWebsite = "site3.com";
            var updatedWebsites = loadedWebsites.ToList();
            updatedWebsites.Add(newWebsite);
            _configurationManager.SaveBlockedWebsites(updatedWebsites);

            // Simulate UI update from configuration change
            blockedWebsites.Add(newWebsite);

            // Assert
            Assert.Contains(newWebsite, blockedWebsites);
            Assert.Equal(updatedWebsites.Count, blockedWebsites.Count);

            // Verify persistence
            var reloadedWebsites = _configurationManager.LoadBlockedWebsites();
            Assert.Contains(newWebsite, reloadedWebsites);
        }

        [Fact]
        public async Task UIDataBinding_RealTimeUpdates_ShouldHandleHighFrequencyChanges()
        {
            // Arrange
            var processCollection = new ObservableCollection<ProcessInfo>();
            var updateCount = 0;
            var maxUpdates = 10;

            processCollection.CollectionChanged += (sender, e) =>
            {
                updateCount++;
            };

            // Act - Simulate high-frequency updates (like timer ticks)
            for (int i = 0; i < maxUpdates; i++)
            {
                var processes = _processMonitor.GetRunningProcesses();
                
                // Clear and repopulate (simulating timer refresh)
                processCollection.Clear();
                
                foreach (var process in processes.Take(3)) // Limit for performance
                {
                    processCollection.Add(process);
                }

                // Small delay to simulate real-world timing
                await Task.Delay(10);
            }

            // Assert
            Assert.True(updateCount >= maxUpdates, $"Should have received at least {maxUpdates} updates, got {updateCount}");
            Assert.True(processCollection.Count >= 0, "Collection should be in valid state");
        }

        [Fact]
        public void UIDataBinding_PropertyValidation_ShouldEnforceDataIntegrity()
        {
            // Arrange
            var processInfo = new ProcessInfo();
            var validationErrors = new List<string>();

            // Act & Assert - Test property validation
            processInfo.ProcessName = "valid_process";
            Assert.Equal("valid_process", processInfo.ProcessName);

            processInfo.Id = 1234;
            Assert.Equal(1234, processInfo.Id);

            processInfo.Status = "Allowed";
            Assert.Equal("Allowed", processInfo.Status);

            processInfo.Status = "Blocked";
            Assert.Equal("Blocked", processInfo.Status);

            // Test LastSeen property
            var testTime = DateTime.Now;
            processInfo.LastSeen = testTime;
            Assert.Equal(testTime, processInfo.LastSeen);
        }

        [Fact]
        public void UIDataBinding_CollectionSynchronization_ShouldMaintainConsistency()
        {
            // Arrange
            var websites = new ObservableCollection<string>();
            var processes = new ObservableCollection<ProcessInfo>();

            // Act - Add items to both collections
            websites.Add("test1.com");
            websites.Add("test2.com");

            processes.Add(new ProcessInfo { ProcessName = "proc1", Id = 1, Status = "Allowed" });
            processes.Add(new ProcessInfo { ProcessName = "proc2", Id = 2, Status = "Blocked" });

            // Simulate concurrent modifications
            websites.Remove("test1.com");
            processes.RemoveAt(0);

            // Assert - Collections should maintain consistency
            Assert.Single(websites);
            Assert.Single(processes);
            Assert.Equal("test2.com", websites[0]);
            Assert.Equal("proc2", processes[0].ProcessName);
        }

        [Fact]
        public void UIDataBinding_MemoryManagement_ShouldNotLeakEventHandlers()
        {
            // Arrange
            var processInfo = new ProcessInfo { ProcessName = "test", Id = 1, Status = "Allowed" };
            var eventHandlerCalled = false;

            PropertyChangedEventHandler handler = (sender, e) =>
            {
                eventHandlerCalled = true;
            };

            // Act - Subscribe and unsubscribe
            processInfo.PropertyChanged += handler;
            processInfo.Status = "Blocked"; // Should trigger event
            Assert.True(eventHandlerCalled, "Event handler should be called");

            // Unsubscribe
            eventHandlerCalled = false;
            processInfo.PropertyChanged -= handler;
            processInfo.Status = "Allowed"; // Should not trigger our handler

            // Assert - Event handler should not be called after unsubscribe
            Assert.False(eventHandlerCalled, "Event handler should not be called after unsubscribe");
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