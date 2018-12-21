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
        public ICollection<User> Accounts { get; set; }
        public ICollection<IdentityRole> AvailableIdentityRoles { get; set; }
        public AccountSearch AccountSearch { get; set; }
        public string ResultMsg { get; set; }
        public int ResultStatus { get; set; }

        public enum ResultStatusList
        {
            UserDeleted = 1,
            UnexpectedError = 2,
            CannotDeleteYourself = 3
        };
    }
}
