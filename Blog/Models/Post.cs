using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class Post
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Slug Url")]
        [StringLength(100, MinimumLength = 1,
            ErrorMessage = "Post slug must be between 3 and 100 characters long!")]
        public string Slug { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Post title must be between 3 and 100 characters long!")]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.Html)]
        public string Content { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [StringLength(300, MinimumLength = 3,
            ErrorMessage = "Post description must be between 3 and 300 characters long!")]
        public string Description { get; set; }

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
        public string AuthorId { get; set; }

        [Required]
        [Display(Name = "Author")]
        public string AuthorUserName { get; set; }

        [Required]
        [Display(Name = "Published")]
        public bool IsPublished { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }
}
