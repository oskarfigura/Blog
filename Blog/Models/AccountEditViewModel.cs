using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Blog.Models
{
    public class AccountEditViewModel
    {
        [Required]
        public User UserAccount { get; set; }
        public ICollection<IdentityRole> AvailableIdentityRoles { get; set; }
    }
}
