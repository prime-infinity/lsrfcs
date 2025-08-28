using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

namespace LaserFocus.Core.Services
{
    /// <summary>
    /// Manages website blocking through Windows hosts file manipulation
    /// </summary>
    public class HostsFileManager
    {
        private readonly string _hostsFilePath;
        private readonly string _backupFilePath;
        private const string REDIRECT_IP = "127.0.0.1";
        private const string LASER_FOCUS_MARKER = "# LaserFocus blocked websites";
        private const string LASER_FOCUS_END_MARKER = "# End LaserFocus blocked websites";

        public HostsFileManager()
        {
            _hostsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), 
                                        "drivers", "etc", "hosts");
            _backupFilePath = _hostsFilePath + ".laserfocus.backup";
        }

        /// <summary>
        /// Blocks a website by adding it to the hosts file
        /// </summary>
        /// <param name="website">Website URL to block</param>
        /// <exception cref="UnauthorizedAccessException">Thrown when administrator privileges are required</exception>
        /// <exception cref="ArgumentException">Thrown when website URL is invalid</exception>
        public void BlockWebsite(string website)
        {
            LoggingService.Instance.LogInfo($"Attempting to block website: {website}", "HostsFileManager.BlockWebsite");
            
            try
            {
                ValidateAdminPrivileges();
                
                // Validate and format the website URL
                var validationResult = InputValidationService.ValidateWebsiteUrl(website);
                if (!validationResult.IsValid)
                {
                    LoggingService.Instance.LogWarning($"Website validation failed: {validationResult.ErrorMessage}", "HostsFileManager.BlockWebsite");
                    throw new ArgumentException(validationResult.ErrorMessage);
                }

                var formattedWebsite = validationResult.FormattedValue!;
                
                if (IsWebsiteBlocked(formattedWebsite))
                {
                    LoggingService.Instance.LogInfo($"Website already blocked: {formattedWebsite}", "HostsFileManager.BlockWebsite");
                    return; // Website is already blocked
                }

                // Create backup before making changes
                CreateBackupIfNotExists();
                
                // Read current hosts file content
                var hostsContent = File.ReadAllText(_hostsFilePath);
                var updatedContent = AddWebsiteToHosts(hostsContent, formattedWebsite);
                
                // Write updated content to hosts file
                File.WriteAllText(_hostsFilePath, updatedContent);
                
                // Flush DNS cache to ensure changes take effect
                FlushDnsCache();
                
                LoggingService.Instance.LogInfo($"Successfully blocked website: {formattedWebsite}", "HostsFileManager.BlockWebsite");
            }
            catch (UnauthorizedAccessException ex)
            {
                LoggingService.Instance.LogException(ex, "HostsFileManager.BlockWebsite", $"Access denied while blocking {website}");
                throw;
            }
            catch (ArgumentException ex)
            {
                LoggingService.Instance.LogException(ex, "HostsFileManager.BlockWebsite", $"Invalid website URL: {website}");
                throw;
            }
            catch (IOException ex)
            {
                LoggingService.Instance.LogException(ex, "HostsFileManager.BlockWebsite", $"File I/O error while blocking {website}");
                throw new InvalidOperationException($"Failed to block website '{website}': File operation failed. The hosts file may be in use by another program.", ex);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "HostsFileManager.BlockWebsite", $"Unexpected error while blocking {website}");
                throw new InvalidOperationException($"Failed to block website '{website}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Unblocks a website by removing it from the hosts file
        /// </summary>
        /// <param name="website">Website URL to unblock</param>
        /// <exception cref="UnauthorizedAccessException">Thrown when administrator privileges are required</exception>
        public void UnblockWebsite(string website)
        {
            LoggingService.Instance.LogInfo($"Attempting to unblock website: {website}", "HostsFileManager.UnblockWebsite");
            
            try
            {
                ValidateAdminPrivileges();
                
                // Validate and format the website URL
                var validationResult = InputValidationService.ValidateWebsiteUrl(website);
                if (!validationResult.IsValid)
                {
                    LoggingService.Instance.LogWarning($"Website validation failed during unblock: {validationResult.ErrorMessage}", "HostsFileManager.UnblockWebsite");
                    throw new ArgumentException(validationResult.ErrorMessage);
                }

                var formattedWebsite = validationResult.FormattedValue!;
                
                if (!IsWebsiteBlocked(formattedWebsite))
                {
                    LoggingService.Instance.LogInfo($"Website not currently blocked: {formattedWebsite}", "HostsFileManager.UnblockWebsite");
                    return; // Website is not blocked
                }

                // Read current hosts file content
                var hostsContent = File.ReadAllText(_hostsFilePath);
                var updatedContent = RemoveWebsiteFromHosts(hostsContent, formattedWebsite);
                
                // Write updated content to hosts file
                File.WriteAllText(_hostsFilePath, updatedContent);
                
                // Flush DNS cache to ensure changes take effect
                FlushDnsCache();
                
                LoggingService.Instance.LogInfo($"Successfully unblocked website: {formattedWebsite}", "HostsFileManager.UnblockWebsite");
            }
            catch (UnauthorizedAccessException ex)
            {
                LoggingService.Instance.LogException(ex, "HostsFileManager.UnblockWebsite", $"Access denied while unblocking {website}");
                throw;
            }
            catch (ArgumentException ex)
            {
                LoggingService.Instance.LogException(ex, "HostsFileManager.UnblockWebsite", $"Invalid website URL: {website}");
                throw;
            }
            catch (IOException ex)
            {
                LoggingService.Instance.LogException(ex, "HostsFileManager.UnblockWebsite", $"File I/O error while unblocking {website}");
                throw new InvalidOperationException($"Failed to unblock website '{website}': File operation failed. The hosts file may be in use by another program.", ex);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "HostsFileManager.UnblockWebsite", $"Unexpected error while unblocking {website}");
                throw new InvalidOperationException($"Failed to unblock website '{website}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if a website is currently blocked
        /// </summary>
        /// <param name="website">Website URL to check</param>
        /// <returns>True if the website is blocked, false otherwise</returns>
        public bool IsWebsiteBlocked(string website)
        {
            try
            {
                LoggingService.Instance.LogInfo($"Checking if website is blocked: {website}", "HostsFileManager.IsWebsiteBlocked");
                
                // Validate and format the website URL
                var validationResult = InputValidationService.ValidateWebsiteUrl(website);
                if (!validationResult.IsValid)
                {
                    LoggingService.Instance.LogWarning($"Invalid website URL during block check: {validationResult.ErrorMessage}", "HostsFileManager.IsWebsiteBlocked");
                    return false; // Invalid URLs are considered not blocked
                }

                var formattedWebsite = validationResult.FormattedValue!;
                
                // Check if hosts file exists
                if (!File.Exists(_hostsFilePath))
                {
                    LoggingService.Instance.LogWarning("Hosts file does not exist", "HostsFileManager.IsWebsiteBlocked");
                    return false;
                }

                var hostsContent = File.ReadAllText(_hostsFilePath);
                var laserFocusSection = ExtractLaserFocusSection(hostsContent);
                var isBlocked = laserFocusSection.Contains($"{REDIRECT_IP} {formattedWebsite}");
                
                LoggingService.Instance.LogInfo($"Website {formattedWebsite} blocked status: {isBlocked}", "HostsFileManager.IsWebsiteBlocked");
                return isBlocked;
            }
            catch (IOException ex)
            {
                LoggingService.Instance.LogException(ex, "HostsFileManager.IsWebsiteBlocked", $"File I/O error checking {website}");
                throw new InvalidOperationException($"Failed to check if website '{website}' is blocked: Unable to read hosts file.", ex);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "HostsFileManager.IsWebsiteBlocked", $"Unexpected error checking {website}");
                throw new InvalidOperationException($"Failed to check if website '{website}' is blocked: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all currently blocked websites
        /// </summary>
        /// <returns>List of blocked website URLs</returns>
        public List<string> GetBlockedWebsites()
        {
            try
            {
                var hostsContent = File.ReadAllText(_hostsFilePath);
                var laserFocusSection = ExtractLaserFocusSection(hostsContent);
                
                var blockedWebsites = new List<string>();
                var lines = laserFocusSection.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var line in lines)
                {
                    if (line.StartsWith(REDIRECT_IP) && !line.StartsWith("#"))
                    {
                        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            blockedWebsites.Add(parts[1]);
                        }
                    }
                }
                
                return blockedWebsites;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get blocked websites: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a backup of the hosts file if one doesn't exist
        /// </summary>
        public void CreateBackupIfNotExists()
        {
            try
            {
                if (!File.Exists(_backupFilePath) && File.Exists(_hostsFilePath))
                {
                    File.Copy(_hostsFilePath, _backupFilePath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create hosts file backup: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Restores the hosts file from backup
        /// </summary>
        /// <exception cref="FileNotFoundException">Thrown when backup file doesn't exist</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when administrator privileges are required</exception>
        public void RestoreFromBackup()
        {
            ValidateAdminPrivileges();
            
            if (!File.Exists(_backupFilePath))
            {
                throw new FileNotFoundException("Backup file not found. Cannot restore hosts file.");
            }

            try
            {
                File.Copy(_backupFilePath, _hostsFilePath, true);
                FlushDnsCache();
            }
            catch (Exception ex) when (!(ex is UnauthorizedAccessException))
            {
                throw new InvalidOperationException($"Failed to restore hosts file from backup: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if the current user has administrator privileges
        /// </summary>
        /// <returns>True if user has admin privileges, false otherwise</returns>
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        public virtual bool HasAdministratorPrivileges()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates that the current user has administrator privileges
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown when administrator privileges are required</exception>
        private void ValidateAdminPrivileges()
        {
            if (!HasAdministratorPrivileges())
            {
                throw new UnauthorizedAccessException("Administrator privileges are required to modify the hosts file.");
            }
        }

        /// <summary>
        /// Formats and validates a website URL
        /// </summary>
        /// <param name="website">Raw website URL</param>
        /// <returns>Formatted website URL</returns>
        /// <exception cref="ArgumentException">Thrown when website URL is invalid</exception>
        private string FormatWebsiteUrl(string website)
        {
            if (string.IsNullOrWhiteSpace(website))
            {
                throw new ArgumentException("Website URL cannot be empty or null.");
            }

            // Remove protocol if present
            website = website.Replace("http://", "").Replace("https://", "").Replace("www.", "");
            
            // Remove trailing slash and path
            var slashIndex = website.IndexOf('/');
            if (slashIndex > 0)
            {
                website = website.Substring(0, slashIndex);
            }

            // Basic domain validation
            var domainRegex = new Regex(@"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$");
            if (!domainRegex.IsMatch(website))
            {
                throw new ArgumentException($"Invalid website URL format: {website}");
            }

            return website.ToLower();
        }

        /// <summary>
        /// Adds a website to the hosts file content
        /// </summary>
        private string AddWebsiteToHosts(string hostsContent, string website)
        {
            var lines = hostsContent.Split(new[] { '\r', '\n' }, StringSplitOptions.None).ToList();
            
            // Find or create LaserFocus section
            var startIndex = lines.FindIndex(line => line.Contains(LASER_FOCUS_MARKER));
            var endIndex = lines.FindIndex(line => line.Contains(LASER_FOCUS_END_MARKER));

            if (startIndex == -1)
            {
                // Create new LaserFocus section
                lines.Add("");
                lines.Add(LASER_FOCUS_MARKER);
                lines.Add($"{REDIRECT_IP} {website}");
                lines.Add(LASER_FOCUS_END_MARKER);
            }
            else
            {
                // Add to existing section
                var insertIndex = endIndex == -1 ? lines.Count : endIndex;
                lines.Insert(insertIndex, $"{REDIRECT_IP} {website}");
                
                if (endIndex == -1)
                {
                    lines.Add(LASER_FOCUS_END_MARKER);
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Removes a website from the hosts file content
        /// </summary>
        private string RemoveWebsiteFromHosts(string hostsContent, string website)
        {
            var lines = hostsContent.Split(new[] { '\r', '\n' }, StringSplitOptions.None).ToList();
            
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                if (lines[i].Contains($"{REDIRECT_IP} {website}"))
                {
                    lines.RemoveAt(i);
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Extracts the LaserFocus section from hosts file content
        /// </summary>
        private string ExtractLaserFocusSection(string hostsContent)
        {
            var lines = hostsContent.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            var startIndex = Array.FindIndex(lines, line => line.Contains(LASER_FOCUS_MARKER));
            var endIndex = Array.FindIndex(lines, line => line.Contains(LASER_FOCUS_END_MARKER));

            if (startIndex == -1)
            {
                return string.Empty;
            }

            var endIdx = endIndex == -1 ? lines.Length : endIndex;
            var sectionLines = lines.Skip(startIndex + 1).Take(endIdx - startIndex - 1);
            
            return string.Join(Environment.NewLine, sectionLines);
        }

        /// <summary>
        /// Flushes the DNS cache to ensure blocked websites take effect immediately
        /// </summary>
        private void FlushDnsCache()
        {
            try
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ipconfig",
                    Arguments = "/flushdns",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };

                using (var process = System.Diagnostics.Process.Start(processInfo))
                {
                    process?.WaitForExit(5000); // Wait up to 5 seconds
                }
            }
            catch
            {
                // DNS flush failure is not critical, continue silently
            }
        }
    }
}