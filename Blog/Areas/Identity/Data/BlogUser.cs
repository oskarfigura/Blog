using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Blog.Areas.Identity.Data
{
    public class BlogUser : IdentityUser
    {
        [Required]
        [PersonalData]
        [StringLength(50, ErrorMessage = "Full name cannot be longer than 50 characters.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required]
        [PersonalData]
        [DataType(DataType.Text)]
        [StringLength(15, MinimumLength = 3,
            ErrorMessage = "Display name must be a minimum of 3 characters and cannot be longer than 15 characters ")]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        [Required]
        [PersonalData]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public override string Email { get; set; }
    }
}
