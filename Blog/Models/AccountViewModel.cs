using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class AccountViewModel
    {
        [Required]
        public User UserAccount { get; set; }
    }
}
