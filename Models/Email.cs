using System.ComponentModel.DataAnnotations;

namespace API.Models {
    public class Email {
        /// <summary>
        /// Gets or sets the email address
        /// </summary>
        [Required]
        [Display(Name = "Email Address")]
        [StringLength(50,MinimumLength = 2,ErrorMessage = "Must be between 5 and 100 characters")]
        public string email { get; set; }
    }
}
