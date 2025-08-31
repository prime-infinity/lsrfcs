namespace LaserFocus.Core.Services
{
    /// <summary>
    /// Interface for user feedback functionality including error messages and confirmation dialogs
    /// </summary>
    public interface IUserFeedbackService
    {
        /// <summary>
        /// Shows a success confirmation message to the user
        /// </summary>
        /// <param name="message">The success message to display</param>
        /// <param name="title">Optional title for the dialog</param>
        void ShowSuccess(string message, string title = "Success");

        /// <summary>
        /// Shows an error message to the user with appropriate formatting
        /// </summary>
        /// <param name="message">The error message to display</param>
        /// <param name="title">Optional title for the dialog</param>
        /// <param name="exception">Optional exception for detailed logging</param>
        void ShowError(string message, string title = "Error", Exception? exception = null);

        /// <summary>
        /// Shows a warning message to the user
        /// </summary>
        /// <param name="message">The warning message to display</param>
        /// <param name="title">Optional title for the dialog</param>
        void ShowWarning(string message, string title = "Warning");

        /// <summary>
        /// Shows a confirmation dialog and returns the user's choice
        /// </summary>
        /// <param name="message">The confirmation message to display</param>
        /// <param name="title">Optional title for the dialog</param>
        /// <returns>True if user clicked Yes, false if user clicked No</returns>
        bool ShowConfirmation(string message, string title = "Confirmation");

        /// <summary>
        /// Shows an informational message to the user
        /// </summary>
        /// <param name="message">The information message to display</param>
        /// <param name="title">Optional title for the dialog</param>
        void ShowInfo(string message, string title = "Information");

        /// <summary>
        /// Shows a detailed error message with technical details for advanced users
        /// </summary>
        /// <param name="userMessage">User-friendly error message</param>
        /// <param name="technicalDetails">Technical details for debugging</param>
        /// <param name="title">Optional title for the dialog</param>
        void ShowDetailedError(string userMessage, string technicalDetails, string title = "Error Details");

        /// <summary>
        /// Gets user-friendly error messages for common exception types
        /// </summary>
        /// <param name="exception">The exception to get a friendly message for</param>
        /// <returns>User-friendly error message</returns>
        string GetFriendlyErrorMessage(Exception exception);
    }

    /// <summary>
    /// Static helper class for user feedback functionality
    /// </summary>
    public static class UserFeedbackHelper
    {
        /// <summary>
        /// Gets user-friendly error messages for common exception types
        /// </summary>
        /// <param name="exception">The exception to get a friendly message for</param>
        /// <returns>User-friendly error message</returns>
        public static string GetFriendlyErrorMessage(Exception exception)
        {
            return exception switch
            {
                System.UnauthorizedAccessException => "Access denied. Administrator privileges are required for this operation. Please restart the application as administrator.",
                
                System.IO.FileNotFoundException => "A required file could not be found. The application may need to be reinstalled or configuration files may be missing.",
                
                System.IO.DirectoryNotFoundException => "A required directory could not be found. Please check that the application is properly installed.",
                
                System.IO.IOException => "A file operation failed. The file may be in use by another program or the disk may be full.",
                
                System.ArgumentException => "Invalid input provided. Please check your input and try again.",
                
                System.InvalidOperationException => "The requested operation cannot be performed at this time. Please try again later.",
                
                System.Security.SecurityException => "Security restrictions prevent this operation. Administrator privileges may be required.",
                
                System.TimeoutException => "The operation timed out. Please check your system resources and try again.",
                
                System.OutOfMemoryException => "The system is running low on memory. Please close other applications and try again.",
                
                _ => "An unexpected error occurred. Please check the application logs for more details."
            };
        }
    }
}