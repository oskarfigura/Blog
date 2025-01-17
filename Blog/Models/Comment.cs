﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class Comment
    {
        [Key]
        [Required]
        public string Id { get; set; }

        [Required]
        public string PostId { get; set; }

        [Required]
        public string AuthorId { get; set; }

        [Required]
        public string AuthorDisplayName { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [StringLength(200, MinimumLength = 3,
        ErrorMessage = "A comment must be between 3 and 200 characters long!")]
        [Display(Name = "Comment")]
        public string Content { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dddd, dd MMMM yyyy}")]
        [Display(Name = "Date Posted")]
        public DateTime PubDate { get; set; }
    }
}
