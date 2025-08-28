namespace LaserFocus.Core.Models
{
    /// <summary>
    /// Represents a blocked website entry
    /// </summary>
    public class BlockedWebsite
    {
        /// <summary>
        /// The URL or domain of the blocked website
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// When the website was added to the block list
        /// </summary>
        public DateTime DateBlocked { get; set; } = DateTime.Now;

        /// <summary>
        /// Whether the block is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}