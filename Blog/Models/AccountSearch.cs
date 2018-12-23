using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Models
{
    // Used for account manager and searching the list
    public class AccountSearch
    {
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        public string Role { get; set; }
    }
}
