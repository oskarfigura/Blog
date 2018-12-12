using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Blog.Models
{
    /**
     * View model used for account manager
     */
    public class AccountManagerViewModel
    {
        public IEnumerable<User> Accounts { get; set; }
        public IEnumerable<IdentityRole> AvailableIdentityRoles { get; set; }
        public AccountSearch AccountSearch { get; set; }
    }
}
