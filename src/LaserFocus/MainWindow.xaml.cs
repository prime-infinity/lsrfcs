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
using LaserFocus.Services;

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
        private readonly IUserFeedbackService _userFeedbackService;
        private bool _isMonitoring = false;
        
        // ObservableCollections for data binding
        private ObservableCollection<string> _blockedWebsites;
        private ObservableCollection<ProcessInfo> _runningProcesses;
        
        // Default allowed processes (will be configurable in future tasks)
        private readonly string[] _allowedProcesses = { "chrome", "code", "kiro", "devenv", "notepad", "LaserFocus" };

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize services
            _hostsFileManager = new HostsFileManager();
            _configurationManager = App.GetConfigurationManager() ?? new ConfigurationManager();
            _processMonitor = new ProcessMonitor(_allowedProcesses);
            _userFeedbackService = new UserFeedbackService();
            
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
            
            LoggingService.Instance.LogInfo($"User attempting to add website: {website}", "MainWindow.AddWebsiteButton_Click");
            
            // Validate input
            if (string.IsNullOrEmpty(website))
            {
                var message = "Please enter a website URL to block.";
                UpdateStatus(message);
                _userFeedbackService.ShowWarning("Please enter a website URL in the text box before clicking 'Block Website'.", "Input Required");
                return;
            }
            
            try
            {
                // Validate website URL format first
                var validationResult = InputValidationService.ValidateWebsiteUrl(website);
                if (!validationResult.IsValid)
                {
                    UpdateStatus($"Invalid website URL: {validationResult.ErrorMessage}");
                    _userFeedbackService.ShowError(validationResult.ErrorMessage, "Invalid Website URL");
                    return;
                }

                var formattedWebsite = validationResult.FormattedValue!;
                
                // Show warnings if any
                if (validationResult.Warnings.Any())
                {
                    var warningMessage = string.Join("\n", validationResult.Warnings);
                    if (!_userFeedbackService.ShowConfirmation($"Warning:\n{warningMessage}\n\nDo you want to continue blocking this website?", "Website Validation Warning"))
                    {
                        UpdateStatus("Website blocking cancelled by user.");
                        return;
                    }
                }
                
                // Check if website is already blocked
                if (BlockedWebsites.Contains(formattedWebsite, StringComparer.OrdinalIgnoreCase))
                {
                    var message = $"Website '{formattedWebsite}' is already in the blocked list.";
                    UpdateStatus(message);
                    _userFeedbackService.ShowInfo(message, "Already Blocked");
                    return;
                }
                
                // Block the website using HostsFileManager
                _hostsFileManager.BlockWebsite(website);
                
                // Add to UI collection
                BlockedWebsites.Add(formattedWebsite);
                
                // Save to configuration
                SaveBlockedWebsitesToConfig();
                
                // Clear input and show success message
                WebsiteUrlTextBox.Clear();
                var successMessage = $"Successfully blocked '{formattedWebsite}'. Website access has been restricted system-wide.";
                UpdateStatus(successMessage);
                _userFeedbackService.ShowSuccess(successMessage, "Website Blocked");
                
                LoggingService.Instance.LogInfo($"Successfully blocked website: {formattedWebsite}", "MainWindow.AddWebsiteButton_Click");
            }
            catch (UnauthorizedAccessException ex)
            {
                LoggingService.Instance.LogException(ex, "MainWindow.AddWebsiteButton_Click", "Access denied blocking website");
                HandlePrivilegeError("block websites");
                _userFeedbackService.ShowError(
                    "Administrator privileges are required to block websites. Please restart the application as administrator.",
                    "Access Denied",
                    ex);
            }
            catch (ArgumentException ex)
            {
                LoggingService.Instance.LogException(ex, "MainWindow.AddWebsiteButton_Click", "Invalid website URL");
                var message = $"Invalid website URL: {ex.Message}";
                UpdateStatus(message);
                _userFeedbackService.ShowError(message, "Invalid Input");
            }
            catch (InvalidOperationException ex)
            {
                LoggingService.Instance.LogException(ex, "MainWindow.AddWebsiteButton_Click", "Operation failed");
                var message = $"Failed to block website: {ex.Message}";
                UpdateStatus(message);
                _userFeedbackService.ShowError(message, "Operation Failed");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "MainWindow.AddWebsiteButton_Click", "Unexpected error blocking website");
                var friendlyMessage = _userFeedbackService.GetFriendlyErrorMessage(ex);
                UpdateStatus($"Failed to block website: {friendlyMessage}");
                _userFeedbackService.ShowDetailedError(
                    "An unexpected error occurred while blocking the website.",
                    ex.Message,
                    "Unexpected Error");
            }
        }

        /// <summary>
        /// Handles removing a website from the blocked list
        /// </summary>
        private void RemoveWebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string website)
            {
                LoggingService.Instance.LogInfo($"User attempting to remove website: {website}", "MainWindow.RemoveWebsiteButton_Click");
                
                // Confirm removal with user
                if (!_userFeedbackService.ShowConfirmation(
                    $"Are you sure you want to unblock '{website}'?\n\nThis will restore access to the website.",
                    "Confirm Unblock"))
                {
                    UpdateStatus("Website unblocking cancelled by user.");
                    return;
                }
                
                try
                {
                    // Unblock the website using HostsFileManager
                    _hostsFileManager.UnblockWebsite(website);
                    
                    // Remove from UI collection
                    BlockedWebsites.Remove(website);
                    
                    // Save to configuration
                    SaveBlockedWebsitesToConfig();
                    
                    var successMessage = $"Successfully unblocked '{website}'. Website access has been restored.";
                    UpdateStatus(successMessage);
                    _userFeedbackService.ShowSuccess(successMessage, "Website Unblocked");
                    
                    LoggingService.Instance.LogInfo($"Successfully unblocked website: {website}", "MainWindow.RemoveWebsiteButton_Click");
                }
                catch (UnauthorizedAccessException ex)
                {
                    LoggingService.Instance.LogException(ex, "MainWindow.RemoveWebsiteButton_Click", "Access denied unblocking website");
                    HandlePrivilegeError("unblock websites");
                    _userFeedbackService.ShowError(
                        "Administrator privileges are required to unblock websites. Please restart the application as administrator.",
                        "Access Denied",
                        ex);
                }
                catch (InvalidOperationException ex)
                {
                    LoggingService.Instance.LogException(ex, "MainWindow.RemoveWebsiteButton_Click", "Operation failed");
                    var message = $"Failed to unblock website: {ex.Message}";
                    UpdateStatus(message);
                    _userFeedbackService.ShowError(message, "Operation Failed");
                }
                catch (Exception ex)
                {
                    LoggingService.Instance.LogException(ex, "MainWindow.RemoveWebsiteButton_Click", "Unexpected error unblocking website");
                    var friendlyMessage = _userFeedbackService.GetFriendlyErrorMessage(ex);
                    UpdateStatus($"Failed to unblock website: {friendlyMessage}");
                    _userFeedbackService.ShowDetailedError(
                        "An unexpected error occurred while unblocking the website.",
                        ex.Message,
                        "Unexpected Error");
                }
            }
        }

        /// <summary>
        /// Handles starting the monitoring process
        /// </summary>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            LoggingService.Instance.LogInfo("User attempting to start monitoring", "MainWindow.StartButton_Click");
            
            try
            {
                // Check privileges before starting monitoring
                bool hasAdminPrivileges = App.HasAdministratorPrivileges();
                
                // Warn user about limited functionality if no admin privileges
                if (!hasAdminPrivileges)
                {
                    if (!_userFeedbackService.ShowConfirmation(
                        "The application is running with limited privileges. Process monitoring and termination may not work properly.\n\nDo you want to continue anyway?",
                        "Limited Privileges Warning"))
                    {
                        UpdateStatus("Monitoring start cancelled by user due to privilege limitations.");
                        return;
                    }
                }
                
                IsMonitoring = true;
                _processTimer.Start();
                
                // Immediately update process list when monitoring starts
                UpdateProcessList();
                
                // Provide appropriate status message based on privileges
                string statusMessage = hasAdminPrivileges
                    ? "Monitoring started with full privileges. Checking processes every 2 seconds and terminating non-allowed applications."
                    : "Monitoring started with limited privileges. Process termination may not work properly.";
                
                UpdateStatus(statusMessage);
                
                // Show success confirmation
                var confirmationMessage = hasAdminPrivileges
                    ? "Process monitoring is now active. Non-allowed applications will be automatically terminated."
                    : "Process monitoring is now active with limited functionality due to insufficient privileges.";
                
                _userFeedbackService.ShowSuccess(confirmationMessage, "Monitoring Started");
                
                LoggingService.Instance.LogInfo($"Monitoring started successfully. Admin privileges: {hasAdminPrivileges}", "MainWindow.StartButton_Click");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "MainWindow.StartButton_Click", "Failed to start monitoring");
                
                IsMonitoring = false;
                var friendlyMessage = _userFeedbackService.GetFriendlyErrorMessage(ex);
                UpdateStatus($"Failed to start monitoring: {friendlyMessage}");
                _userFeedbackService.ShowDetailedError(
                    "Failed to start process monitoring.",
                    ex.Message,
                    "Monitoring Error");
            }
        }

        /// <summary>
        /// Handles stopping the monitoring process
        /// </summary>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            LoggingService.Instance.LogInfo("User attempting to stop monitoring", "MainWindow.StopButton_Click");
            
            try
            {
                IsMonitoring = false;
                _processTimer.Stop();
                
                // Clear process list when monitoring stops
                RunningProcesses.Clear();
                
                var statusMessage = "Monitoring stopped. Application termination disabled, website blocks remain active.";
                UpdateStatus(statusMessage);
                
                _userFeedbackService.ShowSuccess(
                    "Process monitoring has been stopped. Website blocking remains active.",
                    "Monitoring Stopped");
                
                LoggingService.Instance.LogInfo("Monitoring stopped successfully", "MainWindow.StopButton_Click");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "MainWindow.StopButton_Click", "Error stopping monitoring");
                
                var friendlyMessage = _userFeedbackService.GetFriendlyErrorMessage(ex);
                UpdateStatus($"Error stopping monitoring: {friendlyMessage}");
                _userFeedbackService.ShowError(
                    "An error occurred while stopping process monitoring.",
                    "Stop Monitoring Error",
                    ex);
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
                LoggingService.Instance.LogInfo("Initializing MainWindow application", "MainWindow.InitializeApplication");
                
                // Clean up old log files
                LoggingService.Instance.CleanupOldLogs();
                
                // Get privilege status from application level
                bool hasAdminPrivileges = App.HasAdministratorPrivileges();
                
                // Display privilege status
                DisplayPrivilegeStatus(hasAdminPrivileges);
                
                // Load blocked websites from configuration
                LoadBlockedWebsitesFromConfig();
                
                // Update UI state based on privileges
                UpdateUIStateForPrivileges(hasAdminPrivileges);
                
                // Set initial status message
                string statusMessage = hasAdminPrivileges 
                    ? "Application started with full privileges. All features available."
                    : "Application started with limited privileges. Some features may not work.";
                
                UpdateStatus(statusMessage);
                
                LoggingService.Instance.LogInfo($"MainWindow initialization completed. Admin privileges: {hasAdminPrivileges}", "MainWindow.InitializeApplication");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "MainWindow.InitializeApplication", "Critical error during initialization");
                
                var friendlyMessage = _userFeedbackService.GetFriendlyErrorMessage(ex);
                UpdateStatus($"Initialization error: {friendlyMessage}");
                
                _userFeedbackService.ShowDetailedError(
                    "An error occurred during application initialization. Some features may not work properly.",
                    ex.Message,
                    "Initialization Error");
            }
        }

        /// <summary>
        /// Displays the current privilege status to the user
        /// </summary>
        /// <param name="hasAdminPrivileges">Whether the application has admin privileges</param>
        private void DisplayPrivilegeStatus(bool hasAdminPrivileges)
        {
            if (hasAdminPrivileges)
            {
                UpdateStatus("Administrator privileges confirmed. All features available.");
            }
            else
            {
                UpdateStatus("Limited privileges detected. Website blocking and process termination may not work.");
            }
        }

        /// <summary>
        /// Updates UI state based on privilege level
        /// </summary>
        /// <param name="hasAdminPrivileges">Whether the application has admin privileges</param>
        private void UpdateUIStateForPrivileges(bool hasAdminPrivileges)
        {
            // Update UI state
            UpdateUIState();
            
            // If no admin privileges, show warning in UI
            if (!hasAdminPrivileges)
            {
                // Future enhancement: Could disable certain UI elements or show warning indicators
                UpdateStatus("Warning: Running with limited privileges. Some features may not function properly.");
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
                
                // Add websites from configuration and sync with hosts file if admin privileges available
                bool hasAdminPrivileges = App.HasAdministratorPrivileges();
                int syncedCount = 0;
                int failedCount = 0;
                
                foreach (var website in configWebsites)
                {
                    BlockedWebsites.Add(website);
                    
                    // Try to sync with hosts file if we have admin privileges
                    if (hasAdminPrivileges)
                    {
                        try
                        {
                            // Check if website is already blocked in hosts file
                            if (!_hostsFileManager.IsWebsiteBlocked(website))
                            {
                                _hostsFileManager.BlockWebsite(website);
                                syncedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggingService.Instance.LogException(ex, "MainWindow.LoadBlockedWebsitesFromConfig", $"Failed to sync website {website} with hosts file");
                            failedCount++;
                        }
                    }
                }
                
                string statusMessage = $"Loaded {BlockedWebsites.Count} blocked websites from configuration.";
                if (hasAdminPrivileges)
                {
                    statusMessage += $" Synced {syncedCount} websites with hosts file.";
                    if (failedCount > 0)
                    {
                        statusMessage += $" {failedCount} websites failed to sync.";
                    }
                }
                else
                {
                    statusMessage += " Website blocking requires administrator privileges.";
                }
                
                UpdateStatus(statusMessage);
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
        /// Handles privilege-related errors with appropriate user feedback
        /// </summary>
        /// <param name="operation">The operation that failed due to insufficient privileges</param>
        private void HandlePrivilegeError(string operation)
        {
            bool hasAdminPrivileges = App.HasAdministratorPrivileges();
            
            if (!hasAdminPrivileges)
            {
                UpdateStatus($"Cannot {operation}: Administrator privileges required. Please restart as administrator for full functionality.");
            }
            else
            {
                UpdateStatus($"Failed to {operation}: Unexpected privilege error occurred.");
            }
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
            try
            {
                // Perform window-specific cleanup
                PerformWindowCleanup();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Cleanup error: {ex.Message}");
            }
            
            base.OnClosed(e);
        }

        /// <summary>
        /// Performs cleanup specific to the main window
        /// </summary>
        private void PerformWindowCleanup()
        {
            try
            {
                // Stop monitoring timer
                if (_processTimer != null)
                {
                    _processTimer.Stop();
                    _processTimer.Tick -= ProcessTimer_Tick;
                }
                
                // Save current configuration before closing
                SaveCurrentConfiguration();
                
                // Clear collections
                RunningProcesses?.Clear();
                
                UpdateStatus("Window cleanup completed successfully.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during window cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the current application configuration
        /// </summary>
        private void SaveCurrentConfiguration()
        {
            try
            {
                // Save blocked websites
                SaveBlockedWebsitesToConfig();
                
                // Future enhancement: Save other settings like window position, monitoring preferences, etc.
                System.Diagnostics.Debug.WriteLine("Configuration saved successfully.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving configuration: {ex.Message}");
            }
        }

        #endregion
    }
}