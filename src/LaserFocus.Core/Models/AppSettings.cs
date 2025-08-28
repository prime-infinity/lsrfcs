namespace LaserFocus.Core.Models
{
    /// <summary>
    /// Application configuration settings
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// List of websites that should be blocked
        /// </summary>
        public List<string> BlockedWebsites { get; set; } = new List<string>();

        /// <summary>
        /// List of applications that are allowed to run during monitoring
        /// </summary>
        public List<string> AllowedApplications { get; set; } = new List<string>
        {
            "chrome",
            "code", 
            "kiro"
        };

        /// <summary>
        /// Interval in milliseconds for process monitoring checks
        /// </summary>
        public int MonitoringInterval { get; set; } = 2000;

        /// <summary>
        /// Whether the application should start minimized
        /// </summary>
        public bool StartMinimized { get; set; } = false;

        /// <summary>
        /// Whether to show notification messages for actions
        /// </summary>
        public bool ShowNotifications { get; set; } = true;
    }
}