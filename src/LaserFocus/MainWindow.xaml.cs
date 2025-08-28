using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LaserFocus.Core.Models;

namespace LaserFocus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly DispatcherTimer _processTimer;
        private bool _isMonitoring = false;
        
        // ObservableCollections for data binding
        private ObservableCollection<string> _blockedWebsites;
        private ObservableCollection<ProcessInfo> _runningProcesses;
        
        // Default allowed processes (will be configurable in future tasks)
        private readonly string[] _allowedProcesses = { "chrome", "code", "kiro", "devenv", "notepad" };

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize collections
            _blockedWebsites = new ObservableCollection<string>();
            _runningProcesses = new ObservableCollection<ProcessInfo>();
            
            // Initialize timer for process monitoring
            _processTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _processTimer.Tick += ProcessTimer_Tick;
            
            // Set data context for binding
            DataContext = this;
            
            // Update UI state
            UpdateUIState();
            UpdateStatus("Application started. Ready to begin monitoring.");
        }

        #region Properties for Data Binding

        /// <summary>
        /// Collection of blocked websites for UI binding
        /// </summary>
        public ObservableCollection<string> BlockedWebsites
        {
            get => _blockedWebsites;
            set
            {
                _blockedWebsites = value;
                OnPropertyChanged(nameof(BlockedWebsites));
            }
        }

        /// <summary>
        /// Collection of running processes for UI binding
        /// </summary>
        public ObservableCollection<ProcessInfo> RunningProcesses
        {
            get => _runningProcesses;
            set
            {
                _runningProcesses = value;
                OnPropertyChanged(nameof(RunningProcesses));
            }
        }

        /// <summary>
        /// Indicates if monitoring is currently active
        /// </summary>
        public bool IsMonitoring
        {
            get => _isMonitoring;
            set
            {
                _isMonitoring = value;
                OnPropertyChanged(nameof(IsMonitoring));
                UpdateUIState();
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles adding a website to the blocked list
        /// </summary>
        private void AddWebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            string website = WebsiteUrlTextBox.Text?.Trim();
            
            if (string.IsNullOrEmpty(website))
            {
                UpdateStatus("Please enter a website URL to block.");
                return;
            }
            
            // Basic URL validation and formatting
            website = FormatWebsiteUrl(website);
            
            if (BlockedWebsites.Contains(website))
            {
                UpdateStatus($"Website '{website}' is already in the blocked list.");
                return;
            }
            
            // Add to blocked list
            BlockedWebsites.Add(website);
            WebsiteUrlTextBox.Clear();
            
            UpdateStatus($"Added '{website}' to blocked websites list.");
            
            // TODO: Integrate with HostsFileManager in future tasks
        }

        /// <summary>
        /// Handles removing a website from the blocked list
        /// </summary>
        private void RemoveWebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string website)
            {
                BlockedWebsites.Remove(website);
                UpdateStatus($"Removed '{website}' from blocked websites list.");
                
                // TODO: Integrate with HostsFileManager in future tasks
            }
        }

        /// <summary>
        /// Handles starting the monitoring process
        /// </summary>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            IsMonitoring = true;
            _processTimer.Start();
            
            UpdateStatus("Monitoring started. Checking processes every 2 seconds.");
            
            // TODO: Integrate with ProcessMonitor and HostsFileManager in future tasks
        }

        /// <summary>
        /// Handles stopping the monitoring process
        /// </summary>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            IsMonitoring = false;
            _processTimer.Stop();
            
            // Clear process list when monitoring stops
            RunningProcesses.Clear();
            
            UpdateStatus("Monitoring stopped.");
            
            // TODO: Integrate with ProcessMonitor in future tasks
        }

        /// <summary>
        /// Timer tick event for periodic process checking
        /// </summary>
        private void ProcessTimer_Tick(object sender, EventArgs e)
        {
            if (!IsMonitoring) return;
            
            // TODO: Replace with actual ProcessMonitor integration in future tasks
            // For now, simulate process monitoring with dummy data
            UpdateProcessList();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Updates the UI state based on monitoring status
        /// </summary>
        private void UpdateUIState()
        {
            StartButton.IsEnabled = !IsMonitoring;
            StopButton.IsEnabled = IsMonitoring;
        }

        /// <summary>
        /// Updates the status bar message
        /// </summary>
        private void UpdateStatus(string message)
        {
            StatusTextBlock.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        /// <summary>
        /// Formats a website URL for consistent storage
        /// </summary>
        private string FormatWebsiteUrl(string website)
        {
            // Remove protocol if present
            if (website.StartsWith("http://") || website.StartsWith("https://"))
            {
                var uri = new Uri(website);
                website = uri.Host;
            }
            
            // Remove www. prefix if present
            if (website.StartsWith("www."))
            {
                website = website.Substring(4);
            }
            
            return website.ToLower();
        }

        /// <summary>
        /// Updates the process list display (placeholder implementation)
        /// </summary>
        private void UpdateProcessList()
        {
            // TODO: Replace with actual ProcessMonitor integration in future tasks
            // This is a placeholder implementation for UI testing
            
            // Clear existing processes
            RunningProcesses.Clear();
            
            // Add some dummy processes for demonstration
            var dummyProcesses = new[]
            {
                new ProcessInfo { ProcessName = "chrome", Id = 1234, Status = "Allowed", LastSeen = DateTime.Now },
                new ProcessInfo { ProcessName = "code", Id = 5678, Status = "Allowed", LastSeen = DateTime.Now },
                new ProcessInfo { ProcessName = "notepad", Id = 9012, Status = "Allowed", LastSeen = DateTime.Now }
            };
            
            foreach (var process in dummyProcesses)
            {
                RunningProcesses.Add(process);
            }
            
            UpdateStatus($"Process list updated. Found {RunningProcesses.Count} running processes.");
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Cleanup

        protected override void OnClosed(EventArgs e)
        {
            // Stop timer and cleanup resources
            _processTimer?.Stop();
            base.OnClosed(e);
        }

        #endregion
    }
}