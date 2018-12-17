using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Models
{
    public class PostViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Slug Url")]
        public string Slug { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.Html)]
        public string Content { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM-dd-yyyy hh:mm tt}")]
        [Display(Name = "Date Posted")]
        public DateTime PubDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM-dd-yyyy hh:mm tt}")]
        [Display(Name = "Date Edited")]
        public DateTime EditDate { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(200, MinimumLength = 3,
            ErrorMessage = "A comment must be between 3 and 200 characters long!")]
        public string Comment { get; set; }

        public IEnumerable<Comment> Comments { get; set; } = new List<Comment>();

        public string Author { get; set; }
    }
}
