using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace API.Models {
    public class Contact {
        public Contact() {
            addresses = new HashSet<Address>();
            emails = new HashSet<Email>();
            phones = new HashSet<Phone>();
        }

        /// <summary>
        /// Gets or sets the contact identifier
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the contacts's first name
        /// </summary>
        /// <value>The contacts's first name.</value>
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50,MinimumLength = 2,ErrorMessage = "Must be between 2 and 50 characters")]
        public string firstName { get; set; }

        /// <summary>
        /// Gets or sets the contacts's last name
        /// </summary>
        /// <value>The contacts's last name.</value>
        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50,MinimumLength = 2,ErrorMessage = "Must be between 2 and 50 characters")]
        public string lastName { get; set; }

        /// <summary>
        /// Gets or sets the contacts's display name
        /// </summary>
        /// <value>The contacts's display name.</value>
        [Required]
        [Display(Name = "Display Name")]
        [StringLength(50,MinimumLength = 2,ErrorMessage = "Must be between 2 and 50 characters")]
        public string displayName { get; set; }

        /// <summary>
        /// List of addresses for the contact
        /// </summary>
        [Display(Name = "Addresses")]
        public virtual ICollection<Address> addresses { get; set; }

        /// <summary>
        /// List of email addresses for the contact
        /// </summary>
        [Display(Name = "Emails")]
        public virtual ICollection<Email> emails { get; set; }

        /// <summary>
        /// List of email phone numbers for the contact
        /// </summary>
        [Display(Name = "Phones")]
        public virtual ICollection<Phone> phones { get; set; }
    }
}
