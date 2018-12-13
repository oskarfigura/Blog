using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Areas.Identity.Data
{
    public class PostRepo : IPostRepo
    {
        private readonly BlogContext _context;
        private bool _disposed = false;

        public PostRepo(BlogContext context)
        {
            _context = context;
        }

        public async Task<bool> AddPost(PostCreateViewModel postCreateViewModel, User author)
        {
            try
            {
                var post = CreatePost(postCreateViewModel, author);
                await _context.Posts.AddAsync(post);
                Save();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<Post>> GetAllPosts()
        {
            var postList = await _context.Posts.ToListAsync();
            return postList ?? new List<Post>();
        }

        public async Task<Post> GetPostById(string postId)
        {
            var post = await _context.Posts.Where(p => p.Id.Equals(postId)).ToListAsync();
            return post.DefaultIfEmpty(new Post()).FirstOrDefault();
        }

        public async Task<Post> GetPostBySlug(string slug)
        {
            var post = await _context.Posts.Where(p => p.Slug.Equals(slug)).ToListAsync();
            return post.DefaultIfEmpty(new Post()).FirstOrDefault();
        }

        public async Task<IEnumerable<Post>> GetPostsBySearchData(string searchData)
        {
            var postList = await GetAllPosts();

            if (string.IsNullOrEmpty(searchData))
            {
                return postList;
            }

            if (postList.Any())
            {
                postList = postList.Where(x => x.Title.Contains(searchData, StringComparison.OrdinalIgnoreCase));
            }

            return postList;
        }

        public async Task<bool> DeletePost(string id)
        {
            try
            {
                var post = await GetPostById(id);
                if (string.IsNullOrEmpty(post.Id))
                {
                    return false;
                }

                _context.Posts.Remove(post);
                Save();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdatePost(PostEditViewModel postEditViewModel)
        {
            try
            {
                var post = await GetPostById(postEditViewModel.PostId);

                post.Title = postEditViewModel.Title;
                post.Content = postEditViewModel.Content;
                post.Slug = postEditViewModel.Slug;
                post.EditDate = DateTime.Today;

                _context.Posts.Update(post);
                Save();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CheckIfSlugIsUnique(string slug, string postId)
        {
            var postContainingSlug = await _context.Posts
                .Where(p => p.Slug.Equals(slug)
                            && !p.Id.Equals(postId)).ToListAsync();
            return postContainingSlug == null;
        }

        public async void Save()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }

            this._disposed = true;
        }

        /**
         * Create new post from view model
         */
        private static Post CreatePost(PostCreateViewModel post, User author)
        {
            return new Post
            {
                Title = post.Title,
                Content = post.Content,
                Slug = post.Slug,
                IsPublished = post.Publish,
                PubDate = DateTime.Today,
                AuthorUserName = author.UserName,
                AuthorId = author.Role
            };
        }
    }
}