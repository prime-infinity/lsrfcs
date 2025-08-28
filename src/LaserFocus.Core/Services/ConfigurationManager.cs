using System.Text.Json;
using LaserFocus.Core.Models;

namespace LaserFocus.Core.Services
{
    /// <summary>
    /// Manages application configuration persistence and loading
    /// </summary>
    public class ConfigurationManager
    {
        private readonly string _configDirectory;
        private readonly string _blockedWebsitesPath;
        private readonly string _allowedApplicationsPath;
        private readonly string _appSettingsPath;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Initializes a new instance of ConfigurationManager
        /// </summary>
        /// <param name="configDirectory">Directory where configuration files are stored</param>
        public ConfigurationManager(string? configDirectory = null)
        {
            _configDirectory = configDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
            _blockedWebsitesPath = Path.Combine(_configDirectory, "blocked-websites.json");
            _allowedApplicationsPath = Path.Combine(_configDirectory, "allowed-applications.json");
            _appSettingsPath = Path.Combine(_configDirectory, "app-settings.json");

            EnsureConfigDirectoryExists();
        }

        /// <summary>
        /// Loads the list of blocked websites from persistent storage
        /// </summary>
        /// <returns>List of blocked website URLs</returns>
        public List<string> LoadBlockedWebsites()
        {
            try
            {
                if (!File.Exists(_blockedWebsitesPath))
                {
                    return CreateDefaultBlockedWebsites();
                }

                var json = File.ReadAllText(_blockedWebsitesPath);
                var websites = JsonSerializer.Deserialize<List<string>>(json, JsonOptions);
                return websites ?? new List<string>();
            }
            catch (Exception ex)
            {
                // Log error and return default configuration
                Console.WriteLine($"Error loading blocked websites: {ex.Message}");
                return CreateDefaultBlockedWebsites();
            }
        }

        /// <summary>
        /// Saves the list of blocked websites to persistent storage
        /// </summary>
        /// <param name="websites">List of website URLs to block</param>
        public void SaveBlockedWebsites(List<string> websites)
        {
            try
            {
                var json = JsonSerializer.Serialize(websites, JsonOptions);
                File.WriteAllText(_blockedWebsitesPath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save blocked websites: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loads the list of allowed applications from persistent storage
        /// </summary>
        /// <returns>List of allowed application names</returns>
        public List<string> LoadAllowedApplications()
        {
            try
            {
                if (!File.Exists(_allowedApplicationsPath))
                {
                    return CreateDefaultAllowedApplications();
                }

                var json = File.ReadAllText(_allowedApplicationsPath);
                var applications = JsonSerializer.Deserialize<List<string>>(json, JsonOptions);
                return applications ?? CreateDefaultAllowedApplications();
            }
            catch (Exception ex)
            {
                // Log error and return default configuration
                Console.WriteLine($"Error loading allowed applications: {ex.Message}");
                return CreateDefaultAllowedApplications();
            }
        }

        /// <summary>
        /// Saves the list of allowed applications to persistent storage
        /// </summary>
        /// <param name="applications">List of application names to allow</param>
        public void SaveAllowedApplications(List<string> applications)
        {
            try
            {
                var json = JsonSerializer.Serialize(applications, JsonOptions);
                File.WriteAllText(_allowedApplicationsPath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save allowed applications: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loads application settings from persistent storage
        /// </summary>
        /// <returns>Application settings object</returns>
        public AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(_appSettingsPath))
                {
                    return CreateDefaultSettings();
                }

                var json = File.ReadAllText(_appSettingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                return settings ?? CreateDefaultSettings();
            }
            catch (Exception ex)
            {
                // Log error and return default configuration
                Console.WriteLine($"Error loading app settings: {ex.Message}");
                return CreateDefaultSettings();
            }
        }

        /// <summary>
        /// Saves application settings to persistent storage
        /// </summary>
        /// <param name="settings">Application settings to save</param>
        public void SaveSettings(AppSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, JsonOptions);
                File.WriteAllText(_appSettingsPath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save app settings: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Initializes default configuration files if they don't exist
        /// </summary>
        public void InitializeDefaultConfiguration()
        {
            try
            {
                if (!File.Exists(_blockedWebsitesPath))
                {
                    SaveBlockedWebsites(CreateDefaultBlockedWebsites());
                }

                if (!File.Exists(_allowedApplicationsPath))
                {
                    SaveAllowedApplications(CreateDefaultAllowedApplications());
                }

                if (!File.Exists(_appSettingsPath))
                {
                    SaveSettings(CreateDefaultSettings());
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize default configuration: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ensures the configuration directory exists
        /// </summary>
        private void EnsureConfigDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_configDirectory))
                {
                    Directory.CreateDirectory(_configDirectory);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create configuration directory: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates default blocked websites list
        /// </summary>
        /// <returns>Default list of blocked websites</returns>
        private static List<string> CreateDefaultBlockedWebsites()
        {
            return new List<string>
            {
                "youtube.com",
                "facebook.com",
                "twitter.com",
                "reddit.com",
                "instagram.com"
            };
        }

        /// <summary>
        /// Creates default allowed applications list
        /// </summary>
        /// <returns>Default list of allowed applications</returns>
        private static List<string> CreateDefaultAllowedApplications()
        {
            return new List<string>
            {
                "chrome",
                "code",
                "kiro",
                "devenv",
                "notepad"
            };
        }

        /// <summary>
        /// Creates default application settings
        /// </summary>
        /// <returns>Default application settings</returns>
        private static AppSettings CreateDefaultSettings()
        {
            return new AppSettings();
        }
    }
}