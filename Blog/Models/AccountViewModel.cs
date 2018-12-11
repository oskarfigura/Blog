using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Areas.Identity.Data;

namespace Blog.Models
{
    /**
     * Used for displaying account data including the role
     */
    public class AccountViewModel : BlogUser
    {
        public string Role { get; set; }
    }
}