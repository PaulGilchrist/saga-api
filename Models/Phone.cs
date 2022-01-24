using System.ComponentModel.DataAnnotations;

namespace API.Models {
    public class Phone {
        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        [Required]
        [Display(Name = "Phone Number")]
        [StringLength(50,MinimumLength = 2,ErrorMessage = "Must be between 7 and 16 digits")]
        public string phoneNumber { get; set; }
    }
}
