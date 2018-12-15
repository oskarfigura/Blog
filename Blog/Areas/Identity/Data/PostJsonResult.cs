using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Models;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Newtonsoft.Json;

namespace Blog.Areas.Identity.Data
{
    public class PostJsonResult
    {
        [JsonProperty(PropertyName ="posts")]
        public List<Post> Posts { get; set; }
    }
}
