using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Models;

namespace Blog.Areas.Identity.Data
{
    public interface IPostRepo : IDisposable
    {
        Task<bool> AddPost(PostCreateViewModel postCreateViewModel, User author);
        Task<IEnumerable<Post>> GetAllPosts();
        Task<IEnumerable<Post>> GetPostsBySearchData(string searchData);
        Task<Post> GetPostById(string postId);
        Task<Post> GetPostBySlug(string slug);
        Task<bool> DeletePost(string id);
        Task<bool> UpdatePost(PostEditViewModel postEditViewModel);
        Task<bool> CheckIfSlugIsUnique(string slug, string postId);
    }
}
