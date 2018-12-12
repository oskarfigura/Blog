using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Blog.Models
{
    public class AccountViewModel
    {
        public User UserAccount { get; set; }
        public IEnumerable<IdentityRole> AvailableIdentityRoles { get; set; }
    }
}
