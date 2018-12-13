using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Models
{
    public class PostEditViewModel
    {
        [Required]
        public string PostId { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Title")]
        [StringLength(60, MinimumLength = 3,
            ErrorMessage = "Post title must be between 3 and 60 characters long!")]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Slug")]
        [StringLength(60, MinimumLength = 3,
            ErrorMessage = "Post slug must be between 3 and 60 characters long!")]
        public string Slug { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Content")]
        [StringLength(1000, MinimumLength = 3,
            ErrorMessage = "Post must be between 3 and 1000 characters long!")]
        public string Content { get; set; }

        [Required]
        [Display(Name = "Published")]
        public bool Published { get; set; }
    }
}
