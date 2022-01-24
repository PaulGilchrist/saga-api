using System.ComponentModel.DataAnnotations;

namespace API.Models {
    public class Address {
        /// <summary>
        /// Gets or sets the street number and name
        /// </summary>
        [Required]
        [Display(Name = "Street")]
        [StringLength(100,MinimumLength = 4,ErrorMessage = "Must be between 4 and 100 characters")]
        public string street { get; set; }

        /// <summary>
        /// Gets or sets the city name
        /// </summary>
        [Required]
        [Display(Name = "City")]
        [StringLength(50,MinimumLength = 3,ErrorMessage = "Must be between 3 and 50 characters")]
        public string city { get; set; }

        /// <summary>
        /// Gets or sets the state abbreviation
        /// </summary>
        [Required]
        [Display(Name = "State")]
        [StringLength(2,MinimumLength = 2,ErrorMessage = "Must be the state's 2 character abbreviation")]
        public string state { get; set; }

        /// <summary>
        /// Gets or sets the postal code
        /// </summary>
        [Required]
        [Display(Name = "Zip Code")]
        [StringLength(10,MinimumLength = 5,ErrorMessage = "Must be in the format ##### or #####-####")]
        public string zip { get; set; }
    }
}
