using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class PostManagerSearch
    {
        [Display(Name = "Post Title")]
        public string PostTitle { get; set; }
        [Display(Name = "Publish Status")]
        public int PublishStatus { get; set; }

        public enum PublishStatusList
        {
            Any = 1,
            Published = 2,
            Unpublished = 3
        };
    }
}