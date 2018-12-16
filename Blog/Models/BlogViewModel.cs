using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Models
{
    public class BlogViewModel
    {
        public IEnumerable<Post> BlogPosts { get; set; }
    }
}
