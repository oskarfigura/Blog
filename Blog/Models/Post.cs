using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Models
{
    public class Post
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Slug Url")]
        public string SlugUrl { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(60, MinimumLength = 3,
        ErrorMessage = "A title must be between 3 and 60 characters long!")]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(1000, MinimumLength = 3,
        ErrorMessage = "Post must be between 3 and 1000 characters long!")]
        public string Content { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date Posted")]
        public DateTime PubDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date Edited")]
        public DateTime EditDate { get; set; }

        [Required]
        public string AuthorId { get; set; }

        IList<Comment> Comments { get; set; } = new List<Comment>();

    }
}
