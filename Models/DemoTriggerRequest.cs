using System.ComponentModel.DataAnnotations;

namespace FacebookWebhookDemo.Models
{
    /// <summary>
    /// Request model for manually triggering demo leads
    /// </summary>
    public class DemoTriggerRequest
    {
        /// <summary>
        /// Name for the demo lead (optional)
        /// </summary>
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? Name { get; set; }

        /// <summary>
        /// Email for the demo lead (optional)
        /// </summary>
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string? Email { get; set; }

        /// <summary>
        /// Phone number for the demo lead (optional)
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? Phone { get; set; }
    }
}