namespace FacebookWebhookDemo.Models
{
    /// <summary>
    /// Represents a lead captured from Facebook or other sources
    /// </summary>
    public class Lead
    {
        /// <summary>
        /// Unique identifier for the lead
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// When the lead was created/captured
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Lead's full name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Lead's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Lead's phone number
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Source of the lead (Facebook, Manual, etc.)
        /// </summary>
        public string Source { get; set; } = "Facebook";

        /// <summary>
        /// Current status of the lead
        /// </summary>
        public string Status { get; set; } = "New";
    }
}