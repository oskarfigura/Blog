using System.Collections.Generic;

namespace Blog.Models
{
    public class PostManagerViewModel
    {
        public ICollection<Post> BlogPosts { get; set; }
        public PostManagerSearch SearchData { get; set; }
        public bool PostDeleted { get; set; }
        public string ResultMsg { get; set; }
    }
}
