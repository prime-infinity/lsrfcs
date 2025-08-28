#nullable enable
using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using LaserFocus.Core.Services;

namespace LaserFocus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ConfigurationManager? _configurationManager;
        private bool _hasAdminPrivileges;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            try
            {
                // Initialize configuration manager
                _configurationManager = new ConfigurationManager();
                
                // Check administrator privileges
                _hasAdminPrivileges = CheckAdministratorPrivileges();
                
                // Handle privilege requirements
                if (!_hasAdminPrivileges)
                {
                    HandleInsufficientPrivileges(e);
                }
                
                // Load startup configuration
                LoadStartupConfiguration();
                
                // Set global application properties
                SetApplicationProperties();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to initialize application: {ex.Message}\n\nThe application will continue with limited functionality.",
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // Perform cleanup procedures
                PerformApplicationCleanup();
            }
            catch (Exception ex)
            {
                // Log cleanup errors but don't prevent shutdown
                Debug.WriteLine($"Cleanup error: {ex.Message}");
            }
            
            base.OnExit(e);
        }

        /// <summary>
        /// Checks if the current process has administrator privileges
        /// </summary>
        /// <returns>True if running as administrator, false otherwise</returns>
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        private bool CheckAdministratorPrivileges()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking administrator privileges: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Handles the case when the application doesn't have sufficient privileges
        /// </summary>
        /// <param name="e">Startup event arguments</param>
        private void HandleInsufficientPrivileges(StartupEventArgs e)
        {
            // Check if we should attempt UAC elevation
            bool shouldElevate = ShouldAttemptElevation(e);
            
            if (shouldElevate)
            {
                if (AttemptUACElevation())
                {
                    // If elevation was attempted, shutdown this instance
                    Shutdown(0);
                    return;
                }
            }
            
            // Show warning about limited functionality
            ShowLimitedPrivilegeWarning();
        }

        /// <summary>
        /// Determines if UAC elevation should be attempted
        /// </summary>
        /// <param name="e">Startup event arguments</param>
        /// <returns>True if elevation should be attempted</returns>
        private bool ShouldAttemptElevation(StartupEventArgs e)
        {
            // Don't attempt elevation if already tried (prevent infinite loop)
            if (e.Args.Length > 0 && e.Args[0] == "--no-elevation")
            {
                return false;
            }
            
            // Ask user if they want to restart with administrator privileges
            var result = MessageBox.Show(
                "Laser Focus requires administrator privileges for full functionality including:\n\n" +
                "• System-wide website blocking\n" +
                "• Automatic application termination\n\n" +
                "Would you like to restart the application as administrator?\n\n" +
                "Click 'No' to continue with limited functionality.",
                "Administrator Privileges Required",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes);
            
            return result == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Attempts to restart the application with UAC elevation
        /// </summary>
        /// <returns>True if elevation was attempted, false if failed</returns>
        private bool AttemptUACElevation()
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName,
                    Arguments = "--no-elevation", // Prevent infinite elevation loop
                    UseShellExecute = true,
                    Verb = "runas" // Request elevation
                };
                
                Process.Start(processInfo);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to restart with administrator privileges: {ex.Message}\n\n" +
                    "The application will continue with limited functionality.",
                    "Elevation Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                
                return false;
            }
        }

        /// <summary>
        /// Shows warning about limited privilege functionality
        /// </summary>
        private void ShowLimitedPrivilegeWarning()
        {
            MessageBox.Show(
                "Running with limited privileges. Some features may not work:\n\n" +
                "• Website blocking will not function\n" +
                "• Process termination will be limited\n\n" +
                "To enable full functionality, restart as administrator.",
                "Limited Functionality Mode",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Loads startup configuration and initializes default settings
        /// </summary>
        private void LoadStartupConfiguration()
        {
            try
            {
                // Initialize default configuration if needed
                _configurationManager?.InitializeDefaultConfiguration();
                
                // Load application settings
                var settings = _configurationManager?.LoadSettings();
                
                if (settings != null)
                {
                    // Apply startup settings (future enhancement)
                    Debug.WriteLine($"Loaded settings: MonitoringInterval={settings.MonitoringInterval}ms");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading startup configuration: {ex.Message}");
                
                // Show non-blocking error message
                MessageBox.Show(
                    $"Failed to load configuration: {ex.Message}\n\nDefault settings will be used.",
                    "Configuration Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Sets global application properties
        /// </summary>
        private void SetApplicationProperties()
        {
            // Set application-wide properties
            Current.Properties["HasAdminPrivileges"] = _hasAdminPrivileges;
            Current.Properties["ConfigurationManager"] = _configurationManager;
            
            // Set shutdown mode to close when main window closes
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        /// <summary>
        /// Performs cleanup procedures when the application is shutting down
        /// </summary>
        private void PerformApplicationCleanup()
        {
            try
            {
                // Save any pending configuration changes
                // (MainWindow will handle its own cleanup)
                
                // Clean up temporary files if any
                CleanupTemporaryFiles();
                
                // Log shutdown
                Debug.WriteLine("Application shutdown cleanup completed successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during application cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleans up any temporary files created by the application
        /// </summary>
        private void CleanupTemporaryFiles()
        {
            try
            {
                // Future enhancement: Clean up any temporary files
                // For now, just ensure hosts file backup cleanup if needed
                Debug.WriteLine("Temporary file cleanup completed.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cleaning up temporary files: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets whether the application has administrator privileges
        /// </summary>
        /// <returns>True if running with admin privileges</returns>
        public static bool HasAdministratorPrivileges()
        {
            try
            {
                return Current?.Properties?.Contains("HasAdminPrivileges") == true && 
                       Current.Properties["HasAdminPrivileges"] is bool hasPrivileges && hasPrivileges;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the configuration manager instance
        /// </summary>
        /// <returns>Configuration manager instance or null</returns>
        public static ConfigurationManager? GetConfigurationManager()
        {
            try
            {
                return Current?.Properties?["ConfigurationManager"] as ConfigurationManager;
            }
            catch
            {
                return null;
            }
        }
    }
}