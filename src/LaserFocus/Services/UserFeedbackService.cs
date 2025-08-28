#nullable enable
using System;
using System.Windows;
using LaserFocus.Core.Services;

namespace LaserFocus.Services
{
    /// <summary>
    /// Provides centralized user feedback functionality including error messages and confirmation dialogs
    /// </summary>
    public class UserFeedbackService : IUserFeedbackService
    {
        /// <summary>
        /// Shows a success confirmation message to the user
        /// </summary>
        /// <param name="message">The success message to display</param>
        /// <param name="title">Optional title for the dialog</param>
        public void ShowSuccess(string message, string title = "Success")
        {
            try
            {
                LoggingService.Instance.LogInfo($"Success message shown: {message}", "UserFeedbackService");
                
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "UserFeedbackService", "Failed to show success message");
            }
        }

        /// <summary>
        /// Shows an error message to the user with appropriate formatting
        /// </summary>
        /// <param name="message">The error message to display</param>
        /// <param name="title">Optional title for the dialog</param>
        /// <param name="exception">Optional exception for detailed logging</param>
        public void ShowError(string message, string title = "Error", Exception? exception = null)
        {
            try
            {
                if (exception != null)
                {
                    LoggingService.Instance.LogException(exception, "UserFeedbackService", message);
                }
                else
                {
                    LoggingService.Instance.LogError($"Error message shown: {message}", "UserFeedbackService");
                }

                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "UserFeedbackService", "Failed to show error message");
            }
        }

        /// <summary>
        /// Shows a warning message to the user
        /// </summary>
        /// <param name="message">The warning message to display</param>
        /// <param name="title">Optional title for the dialog</param>
        public void ShowWarning(string message, string title = "Warning")
        {
            try
            {
                LoggingService.Instance.LogWarning($"Warning message shown: {message}", "UserFeedbackService");
                
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "UserFeedbackService", "Failed to show warning message");
            }
        }

        /// <summary>
        /// Shows a confirmation dialog and returns the user's choice
        /// </summary>
        /// <param name="message">The confirmation message to display</param>
        /// <param name="title">Optional title for the dialog</param>
        /// <returns>True if user clicked Yes, false if user clicked No</returns>
        public bool ShowConfirmation(string message, string title = "Confirmation")
        {
            try
            {
                LoggingService.Instance.LogInfo($"Confirmation dialog shown: {message}", "UserFeedbackService");
                
                var result = MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);

                var userChoice = result == MessageBoxResult.Yes;
                LoggingService.Instance.LogInfo($"User confirmation result: {userChoice}", "UserFeedbackService");
                
                return userChoice;
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "UserFeedbackService", "Failed to show confirmation dialog");
                return false;
            }
        }

        /// <summary>
        /// Shows an informational message to the user
        /// </summary>
        /// <param name="message">The information message to display</param>
        /// <param name="title">Optional title for the dialog</param>
        public void ShowInfo(string message, string title = "Information")
        {
            try
            {
                LoggingService.Instance.LogInfo($"Info message shown: {message}", "UserFeedbackService");
                
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "UserFeedbackService", "Failed to show info message");
            }
        }

        /// <summary>
        /// Shows a detailed error message with technical details for advanced users
        /// </summary>
        /// <param name="userMessage">User-friendly error message</param>
        /// <param name="technicalDetails">Technical details for debugging</param>
        /// <param name="title">Optional title for the dialog</param>
        public void ShowDetailedError(string userMessage, string technicalDetails, string title = "Error Details")
        {
            try
            {
                LoggingService.Instance.LogError($"Detailed error shown - User: {userMessage}, Technical: {technicalDetails}", "UserFeedbackService");
                
                var fullMessage = $"{userMessage}\n\nTechnical Details:\n{technicalDetails}";
                
                MessageBox.Show(
                    fullMessage,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogException(ex, "UserFeedbackService", "Failed to show detailed error message");
            }
        }

        /// <summary>
        /// Gets user-friendly error messages for common exception types
        /// </summary>
        /// <param name="exception">The exception to get a friendly message for</param>
        /// <returns>User-friendly error message</returns>
        public string GetFriendlyErrorMessage(Exception exception)
        {
            return UserFeedbackHelper.GetFriendlyErrorMessage(exception);
        }
    }
}