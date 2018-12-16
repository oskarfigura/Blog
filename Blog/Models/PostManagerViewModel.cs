using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Models
{
    public class PostManagerViewModel
    {
        public IEnumerable<Post> BlogPosts { get; set; }
        public PostManagerSearch SearchData { get; set; }
    }
}
