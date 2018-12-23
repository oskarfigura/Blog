using System.Collections.Generic;
using Blog.Models;
using Newtonsoft.Json;

namespace Blog.Areas.Identity.Data
{
    public class PostJsonResult
    {
        [JsonProperty(PropertyName ="posts")]
        public List<Post> Posts { get; set; }
    }
}
