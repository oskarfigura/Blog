using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Models
{
    public class Comment
    {
        [Key]
        [Required]
        public string Id { get; set; }

        [Required]
        public Post Post { get; set; }

        //Only used to track which account was used when posting
        [Required]
        public string AuthorId { get; set; }

        //Used in cases when the user deletes his account
        [Required]
        public string AuthorDisplayName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(100, MinimumLength = 3,
        ErrorMessage = "A comment must be between 3 and 100 characters long!")]
        [Display(Name = "Comment")]
        public string Content { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dddd, dd MMMM yyyy}")]
        [Display(Name = "Date Posted")]
        public DateTime PubDate { get; set; }
    }
}
