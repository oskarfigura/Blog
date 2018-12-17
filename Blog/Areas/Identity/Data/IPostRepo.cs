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
        Task<IEnumerable<Post>> GetAllPublishedPosts();
        Task<IEnumerable<Post>> GetPostsBySearchData(PostManagerSearch searchData);
        Task<Post> GetPostById(string postId);
        Task<Post> GetPostBySlug(string slug);
        Task<Post> GetPublishedPostBySlug(string slug);
        Task<bool> DeletePost(string id);
        Task<bool> UpdatePost(PostEditViewModel postEditViewModel);
        Task<bool> CheckIfSlugIsUnique(string slug, string postId);
        Task<bool> CheckIfSlugIsUnique(string slug);
        Task<string> AddComment(Comment comment);
        Task<bool> DeleteComment(string id);
    }
}
