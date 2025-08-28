using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LaserFocus.Core.Services
{
    /// <summary>
    /// Provides comprehensive input validation functionality
    /// </summary>
    public static class InputValidationService
    {
        private static readonly Regex DomainRegex = new(
            @"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex IpAddressRegex = new(
            @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$",
            RegexOptions.Compiled);

        private static readonly HashSet<string> ReservedDomains = new(StringComparer.OrdinalIgnoreCase)
        {
            "localhost",
            "127.0.0.1",
            "0.0.0.0",
            "::1"
        };

        private static readonly HashSet<string> CommonTlds = new(StringComparer.OrdinalIgnoreCase)
        {
            "com", "org", "net", "edu", "gov", "mil", "int", "co", "io", "ai", "app", "dev",
            "uk", "ca", "au", "de", "fr", "jp", "cn", "ru", "br", "in", "mx", "es", "it"
        };

        /// <summary>
        /// Validation result containing success status and error details
        /// </summary>
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; } = string.Empty;
            public string? FormattedValue { get; set; }
            public List<string> Warnings { get; set; } = new();
        }

        /// <summary>
        /// Validates and formats a website URL for blocking
        /// </summary>
        /// <param name="website">The website URL to validate</param>
        /// <returns>Validation result with formatted URL if valid</returns>
        public static ValidationResult ValidateWebsiteUrl(string website)
        {
            var result = new ValidationResult();

            try
            {
                LoggingService.Instance.LogInfo($"Validating website URL: {website}", "InputValidationService");

                // Check for null or empty input
                if (string.IsNullOrWhiteSpace(website))
                {
                    result.ErrorMessage = "Website URL cannot be empty. Please enter a valid website address.";
                    return result;
                }

                // Remove common prefixes and clean the input
                var cleanedWebsite = CleanWebsiteUrl(website);
                
                // Check minimum length
                if (cleanedWebsite.Length < 3)
                {
                    result.ErrorMessage = "Website URL is too short. Please enter a valid domain name.";
                    return result;
                }

                // Check maximum length
                if (cleanedWebsite.Length > 253)
                {
                    result.ErrorMessage = "Website URL is too long. Domain names cannot exceed 253 characters.";
                    return result;
                }

                // Check for reserved domains
                if (ReservedDomains.Contains(cleanedWebsite))
                {
                    result.ErrorMessage = "Cannot block reserved domains like localhost or loopback addresses.";
                    return result;
                }

                // Validate IP addresses
                if (IpAddressRegex.IsMatch(cleanedWebsite))
                {
                    if (IsPrivateIpAddress(cleanedWebsite))
                    {
                        result.ErrorMessage = "Cannot block private IP addresses (192.168.x.x, 10.x.x.x, 172.16-31.x.x).";
                        return result;
                    }
                    
                    result.IsValid = true;
                    result.FormattedValue = cleanedWebsite;
                    result.Warnings.Add("Blocking IP addresses may not work as expected. Consider using domain names instead.");
                    return result;
                }

                // Validate domain format
                if (!DomainRegex.IsMatch(cleanedWebsite))
                {
                    result.ErrorMessage = GetDomainFormatErrorMessage(cleanedWebsite);
                    return result;
                }

                // Check for valid TLD
                var parts = cleanedWebsite.Split('.');
                if (parts.Length < 2)
                {
                    result.ErrorMessage = "Invalid domain format. Please include a top-level domain (e.g., .com, .org).";
                    return result;
                }

                var tld = parts[^1];
                if (!IsValidTld(tld))
                {
                    result.Warnings.Add($"'{tld}' is not a common top-level domain. Please verify this is correct.");
                }

                // Check for suspicious patterns
                var suspiciousPatterns = DetectSuspiciousPatterns(cleanedWebsite);
                result.Warnings.AddRange(suspiciousPatterns);

                result.IsValid = true;
                result.FormattedValue = cleanedWebsite.ToLower();
                
                LoggingService.Instance.LogInfo($"Website URL validation successful: {result.FormattedValue}", "InputValidationService");
                return result;
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "InputValidationService", $"Error validating website URL: {website}");
                result.ErrorMessage = "An error occurred while validating the website URL. Please try again.";
                return result;
            }
        }

        /// <summary>
        /// Cleans and normalizes a website URL
        /// </summary>
        /// <param name="website">Raw website URL</param>
        /// <returns>Cleaned website URL</returns>
        private static string CleanWebsiteUrl(string website)
        {
            // Remove protocol prefixes
            website = website.Replace("http://", "").Replace("https://", "");
            
            // Remove www prefix
            if (website.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            {
                website = website[4..];
            }
            
            // Remove trailing slash and path
            var slashIndex = website.IndexOf('/');
            if (slashIndex > 0)
            {
                website = website[..slashIndex];
            }

            // Remove port numbers
            var colonIndex = website.LastIndexOf(':');
            if (colonIndex > 0 && colonIndex < website.Length - 1)
            {
                var portPart = website[(colonIndex + 1)..];
                if (int.TryParse(portPart, out _))
                {
                    website = website[..colonIndex];
                }
            }

            return website.Trim().ToLower();
        }

        /// <summary>
        /// Checks if an IP address is in a private range
        /// </summary>
        /// <param name="ipAddress">IP address to check</param>
        /// <returns>True if the IP is in a private range</returns>
        private static bool IsPrivateIpAddress(string ipAddress)
        {
            var parts = ipAddress.Split('.');
            if (parts.Length != 4 || !parts.All(part => int.TryParse(part, out _)))
            {
                return false;
            }

            var octets = parts.Select(int.Parse).ToArray();

            // Check private IP ranges
            return (octets[0] == 10) ||
                   (octets[0] == 172 && octets[1] >= 16 && octets[1] <= 31) ||
                   (octets[0] == 192 && octets[1] == 168) ||
                   (octets[0] == 127); // Loopback
        }

        /// <summary>
        /// Validates if a TLD is commonly used
        /// </summary>
        /// <param name="tld">Top-level domain to validate</param>
        /// <returns>True if the TLD is commonly used</returns>
        private static bool IsValidTld(string tld)
        {
            return CommonTlds.Contains(tld) || tld.Length >= 2;
        }

        /// <summary>
        /// Gets a specific error message for domain format issues
        /// </summary>
        /// <param name="domain">The invalid domain</param>
        /// <returns>Specific error message</returns>
        private static string GetDomainFormatErrorMessage(string domain)
        {
            if (domain.Contains(".."))
            {
                return "Invalid domain format: consecutive dots are not allowed.";
            }

            if (domain.StartsWith('.') || domain.EndsWith('.'))
            {
                return "Invalid domain format: domain cannot start or end with a dot.";
            }

            if (domain.Contains(' '))
            {
                return "Invalid domain format: spaces are not allowed in domain names.";
            }

            if (domain.Any(c => !char.IsLetterOrDigit(c) && c != '.' && c != '-'))
            {
                return "Invalid domain format: only letters, numbers, dots, and hyphens are allowed.";
            }

            if (domain.Contains("--"))
            {
                return "Invalid domain format: consecutive hyphens are not allowed.";
            }

            return "Invalid domain format. Please enter a valid domain name (e.g., example.com).";
        }

        /// <summary>
        /// Detects suspicious patterns in domain names
        /// </summary>
        /// <param name="domain">Domain to analyze</param>
        /// <returns>List of warning messages</returns>
        private static List<string> DetectSuspiciousPatterns(string domain)
        {
            var warnings = new List<string>();

            // Check for very long subdomains
            var parts = domain.Split('.');
            if (parts.Any(part => part.Length > 63))
            {
                warnings.Add("Domain contains very long subdomain parts. Please verify this is correct.");
            }

            // Check for excessive subdomains
            if (parts.Length > 5)
            {
                warnings.Add("Domain has many subdomain levels. Please verify this is the correct address.");
            }

            // Check for numeric-only domains
            if (parts.Take(parts.Length - 1).Any(part => part.All(char.IsDigit)))
            {
                warnings.Add("Domain contains numeric-only parts. Please verify this is correct.");
            }

            return warnings;
        }
    }
}