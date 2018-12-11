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
        public List<AccountViewModel> Accounts { get; set; }
        public List<IdentityRole> AvailableIdentityRoles { get; set; }
    }
}
