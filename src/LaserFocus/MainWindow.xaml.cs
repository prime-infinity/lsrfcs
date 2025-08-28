using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LaserFocus.Core.Models;
using LaserFocus.Core.Services;

namespace LaserFocus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly DispatcherTimer _processTimer;
        private readonly HostsFileManager _hostsFileManager;
        private readonly ConfigurationManager _configurationManager;
        private readonly ProcessMonitor _processMonitor;
        private bool _isMonitoring = false;
        
        // ObservableCollections for data binding
        private ObservableCollection<string> _blockedWebsites;
        private ObservableCollection<ProcessInfo> _runningProcesses;
        
        // Default allowed processes (will be configurable in future tasks)
        private readonly string[] _allowedProcesses = { "chrome", "code", "kiro", "devenv", "notepad" };

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize services
            _hostsFileManager = new HostsFileManager();
            _configurationManager = new ConfigurationManager();
            _processMonitor = new ProcessMonitor(_allowedProcesses);
            
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
            
            // Load configuration and initialize
            InitializeApplication();
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
            
            try
            {
                // Check if website is already blocked
                if (BlockedWebsites.Contains(website, StringComparer.OrdinalIgnoreCase))
                {
                    UpdateStatus($"Website '{website}' is already in the blocked list.");
                    return;
                }
                
                // Block the website using HostsFileManager
                _hostsFileManager.BlockWebsite(website);
                
                // Add to UI collection
                BlockedWebsites.Add(website);
                
                // Save to configuration
                SaveBlockedWebsitesToConfig();
                
                // Clear input and show success message
                WebsiteUrlTextBox.Clear();
                UpdateStatus($"Successfully blocked '{website}'. Website access has been restricted.");
            }
            catch (UnauthorizedAccessException)
            {
                UpdateStatus("Administrator privileges required to block websites. Please run as administrator.");
            }
            catch (ArgumentException ex)
            {
                UpdateStatus($"Invalid website URL: {ex.Message}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to block website: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles removing a website from the blocked list
        /// </summary>
        private void RemoveWebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string website)
            {
                try
                {
                    // Unblock the website using HostsFileManager
                    _hostsFileManager.UnblockWebsite(website);
                    
                    // Remove from UI collection
                    BlockedWebsites.Remove(website);
                    
                    // Save to configuration
                    SaveBlockedWebsitesToConfig();
                    
                    UpdateStatus($"Successfully unblocked '{website}'. Website access has been restored.");
                }
                catch (UnauthorizedAccessException)
                {
                    UpdateStatus("Administrator privileges required to unblock websites. Please run as administrator.");
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Failed to unblock website: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handles starting the monitoring process
        /// </summary>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsMonitoring = true;
                _processTimer.Start();
                
                // Immediately update process list when monitoring starts
                UpdateProcessList();
                
                UpdateStatus("Monitoring started. Checking processes every 2 seconds and terminating non-allowed applications.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to start monitoring: {ex.Message}");
                IsMonitoring = false;
            }
        }

        /// <summary>
        /// Handles stopping the monitoring process
        /// </summary>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsMonitoring = false;
                _processTimer.Stop();
                
                // Clear process list when monitoring stops
                RunningProcesses.Clear();
                
                UpdateStatus("Monitoring stopped. Application termination disabled, website blocks remain active.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error stopping monitoring: {ex.Message}");
            }
        }

        /// <summary>
        /// Timer tick event for periodic process checking
        /// </summary>
        private void ProcessTimer_Tick(object sender, EventArgs e)
        {
            if (!IsMonitoring) return;
            
            // Update process list and terminate blocked applications
            UpdateProcessList();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Initializes the application by loading configuration and checking privileges
        /// </summary>
        private void InitializeApplication()
        {
            try
            {
                // Check administrator privileges
                CheckAdministratorPrivileges();
                
                // Initialize default configuration if needed
                _configurationManager.InitializeDefaultConfiguration();
                
                // Load blocked websites from configuration
                LoadBlockedWebsitesFromConfig();
                
                // Update UI state
                UpdateUIState();
                UpdateStatus("Application started. Ready to begin monitoring.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Initialization error: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if the application has administrator privileges
        /// </summary>
        private void CheckAdministratorPrivileges()
        {
            if (!_hostsFileManager.HasAdministratorPrivileges())
            {
                UpdateStatus("Warning: Administrator privileges not detected. Website blocking may not work properly.");
            }
            else
            {
                UpdateStatus("Administrator privileges confirmed. All features available.");
            }
        }

        /// <summary>
        /// Loads blocked websites from configuration and syncs with hosts file
        /// </summary>
        private void LoadBlockedWebsitesFromConfig()
        {
            try
            {
                var configWebsites = _configurationManager.LoadBlockedWebsites();
                
                // Clear current collection
                BlockedWebsites.Clear();
                
                // Add websites from configuration
                foreach (var website in configWebsites)
                {
                    BlockedWebsites.Add(website);
                }
                
                UpdateStatus($"Loaded {BlockedWebsites.Count} blocked websites from configuration.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to load blocked websites: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves current blocked websites to configuration
        /// </summary>
        private void SaveBlockedWebsitesToConfig()
        {
            try
            {
                var websitesList = BlockedWebsites.ToList();
                _configurationManager.SaveBlockedWebsites(websitesList);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to save configuration: {ex.Message}");
            }
        }

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
        /// Updates the process list display using ProcessMonitor
        /// </summary>
        private void UpdateProcessList()
        {
            try
            {
                // Get current running processes from ProcessMonitor
                var currentProcesses = _processMonitor.GetRunningProcesses();
                
                // Clear existing processes
                RunningProcesses.Clear();
                
                // Add current processes to the UI collection
                foreach (var process in currentProcesses)
                {
                    RunningProcesses.Add(process);
                    
                    // If process is blocked, attempt to terminate it
                    if (process.Status == "Blocked")
                    {
                        TerminateBlockedProcess(process);
                    }
                }
                
                UpdateStatus($"Process list updated. Found {RunningProcesses.Count} running processes.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error updating process list: {ex.Message}");
            }
        }

        /// <summary>
        /// Terminates a blocked process with error handling
        /// </summary>
        /// <param name="processInfo">The process information for the process to terminate</param>
        private void TerminateBlockedProcess(ProcessInfo processInfo)
        {
            try
            {
                bool terminated = _processMonitor.TerminateProcess(processInfo.Id);
                
                if (terminated)
                {
                    UpdateStatus($"Terminated blocked application: {processInfo.ProcessName} (ID: {processInfo.Id})");
                }
                else
                {
                    UpdateStatus($"Failed to terminate {processInfo.ProcessName} (ID: {processInfo.Id}) - may be protected or already closed");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error terminating process {processInfo.ProcessName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the process monitoring action log for debugging purposes
        /// </summary>
        /// <returns>List of process action log entries</returns>
        public List<string> GetProcessActionLog()
        {
            return _processMonitor.GetProcessActionLog();
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