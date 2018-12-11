using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Models
{
    /**
     * Model used when using account manager and searching the list
     */
    public class AccountSearchModel
    {
        public string UserName { get; set; }
        public string Role { get; set; }
    }
}
