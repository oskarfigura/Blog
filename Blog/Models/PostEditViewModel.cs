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
        public string Title { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Slug")]
        public string Slug { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Content")]
        public string Content { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Published")]
        public bool Published { get; set; }
    }
}
