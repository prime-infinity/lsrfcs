using LaserFocus.Core.Models;
using LaserFocus.Core.Services;

namespace LaserFocus.Core.Services
{
    /// <summary>
    /// Demo class to test ConfigurationManager functionality
    /// </summary>
    public static class ConfigurationManagerDemo
    {
        /// <summary>
        /// Demonstrates the ConfigurationManager functionality
        /// </summary>
        public static void RunDemo()
        {
            Console.WriteLine("=== ConfigurationManager Demo ===");
            
            // Initialize configuration manager
            var configManager = new ConfigurationManager();
            
            // Initialize default configuration
            Console.WriteLine("Initializing default configuration...");
            configManager.InitializeDefaultConfiguration();
            
            // Load and display blocked websites
            Console.WriteLine("\nLoading blocked websites:");
            var blockedWebsites = configManager.LoadBlockedWebsites();
            foreach (var website in blockedWebsites)
            {
                Console.WriteLine($"  - {website}");
            }
            
            // Load and display allowed applications
            Console.WriteLine("\nLoading allowed applications:");
            var allowedApps = configManager.LoadAllowedApplications();
            foreach (var app in allowedApps)
            {
                Console.WriteLine($"  - {app}");
            }
            
            // Load and display settings
            Console.WriteLine("\nLoading application settings:");
            var settings = configManager.LoadSettings();
            Console.WriteLine($"  Monitoring Interval: {settings.MonitoringInterval}ms");
            Console.WriteLine($"  Start Minimized: {settings.StartMinimized}");
            Console.WriteLine($"  Show Notifications: {settings.ShowNotifications}");
            
            // Add a new blocked website
            Console.WriteLine("\nAdding new blocked website...");
            blockedWebsites.Add("example.com");
            configManager.SaveBlockedWebsites(blockedWebsites);
            
            // Reload and verify
            var updatedWebsites = configManager.LoadBlockedWebsites();
            Console.WriteLine("Updated blocked websites:");
            foreach (var website in updatedWebsites)
            {
                Console.WriteLine($"  - {website}");
            }
            
            Console.WriteLine("\n=== Demo Complete ===");
        }
    }
}