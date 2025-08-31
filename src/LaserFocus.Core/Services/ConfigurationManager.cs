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
                LoggingService.Instance.LogInfo($"Loading blocked websites from: {_blockedWebsitesPath}", "ConfigurationManager.LoadBlockedWebsites");
                
                if (!File.Exists(_blockedWebsitesPath))
                {
                    LoggingService.Instance.LogInfo("Blocked websites file not found, creating default configuration", "ConfigurationManager.LoadBlockedWebsites");
                    var defaultWebsites = CreateDefaultBlockedWebsites();
                    SaveBlockedWebsites(defaultWebsites); // Save default configuration
                    return defaultWebsites;
                }

                var json = File.ReadAllText(_blockedWebsitesPath);
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    LoggingService.Instance.LogWarning("Blocked websites file is empty, using default configuration", "ConfigurationManager.LoadBlockedWebsites");
                    return CreateDefaultBlockedWebsites();
                }

                var websites = JsonSerializer.Deserialize<List<string>>(json, JsonOptions);
                var result = websites ?? new List<string>();
                
                // If the list is empty, use default websites for first-time setup
                if (result.Count == 0)
                {
                    LoggingService.Instance.LogInfo("Blocked websites list is empty, using default configuration", "ConfigurationManager.LoadBlockedWebsites");
                    result = CreateDefaultBlockedWebsites();
                    SaveBlockedWebsites(result); // Save the default websites
                }
                
                LoggingService.Instance.LogInfo($"Successfully loaded {result.Count} blocked websites", "ConfigurationManager.LoadBlockedWebsites");
                return result;
            }
            catch (JsonException ex)
            {
                LoggingService.Instance.LogException(ex, "ConfigurationManager.LoadBlockedWebsites", "JSON deserialization error");
                LoggingService.Instance.LogWarning("Using default blocked websites due to JSON error", "ConfigurationManager.LoadBlockedWebsites");
                return CreateDefaultBlockedWebsites();
            }
            catch (IOException ex)
            {
                LoggingService.Instance.LogException(ex, "ConfigurationManager.LoadBlockedWebsites", "File I/O error");
                LoggingService.Instance.LogWarning("Using default blocked websites due to file error", "ConfigurationManager.LoadBlockedWebsites");
                return CreateDefaultBlockedWebsites();
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "ConfigurationManager.LoadBlockedWebsites", "Unexpected error loading blocked websites");
                LoggingService.Instance.LogWarning("Using default blocked websites due to unexpected error", "ConfigurationManager.LoadBlockedWebsites");
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
                LoggingService.Instance.LogInfo($"Saving {websites?.Count ?? 0} blocked websites to: {_blockedWebsitesPath}", "ConfigurationManager.SaveBlockedWebsites");
                
                if (websites == null)
                {
                    LoggingService.Instance.LogWarning("Null websites list provided, saving empty list", "ConfigurationManager.SaveBlockedWebsites");
                    websites = new List<string>();
                }

                EnsureConfigDirectoryExists();
                
                var json = JsonSerializer.Serialize(websites, JsonOptions);
                File.WriteAllText(_blockedWebsitesPath, json);
                
                LoggingService.Instance.LogInfo("Successfully saved blocked websites configuration", "ConfigurationManager.SaveBlockedWebsites");
            }
            catch (IOException ex)
            {
                LoggingService.Instance.LogException(ex, "ConfigurationManager.SaveBlockedWebsites", "File I/O error saving blocked websites");
                throw new InvalidOperationException($"Failed to save blocked websites: Unable to write to configuration file. The file may be in use or the disk may be full.", ex);
            }
            catch (JsonException ex)
            {
                LoggingService.Instance.LogException(ex, "ConfigurationManager.SaveBlockedWebsites", "JSON serialization error");
                throw new InvalidOperationException($"Failed to save blocked websites: Configuration data could not be serialized.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                LoggingService.Instance.LogException(ex, "ConfigurationManager.SaveBlockedWebsites", "Access denied saving blocked websites");
                throw new InvalidOperationException($"Failed to save blocked websites: Access denied. Check file permissions.", ex);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "ConfigurationManager.SaveBlockedWebsites", "Unexpected error saving blocked websites");
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