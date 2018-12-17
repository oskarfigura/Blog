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
        private bool _disposed;

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
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> AddComment(Comment comment)
        {
            if (string.IsNullOrEmpty(comment.Content)) return null;

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment.Id;
        }

        public async Task<IEnumerable<Post>> GetAllPosts()
        {
            var postList = await _context.Posts.AsNoTracking()
                .Include(p => p.Comments).ToListAsync();
            return postList ?? new List<Post>();
        }

        public async Task<IEnumerable<Post>> GetAllPublishedPosts()
        {
            var postList = await _context.Posts.Where(p => p.IsPublished).AsNoTracking()
                .Include(p => p.Comments).ToListAsync();
            return postList ?? new List<Post>();
        }

        public async Task<Post> GetPostById(string postId)
        {
            var post = await _context.Posts.Where(p => p.Id.Equals(postId)).AsNoTracking()
                .Include(p => p.Comments).ToListAsync();
            return post.DefaultIfEmpty(new Post()).FirstOrDefault();
        }

        public async Task<Post> GetPostBySlug(string slug)
        {
            var post = await _context.Posts.Where(p => p.Slug.Equals(slug)).AsNoTracking()
                .Include(p => p.Comments).ToListAsync();
            return post.DefaultIfEmpty(new Post()).FirstOrDefault();
        }

        public async Task<Post> GetPublishedPostBySlug(string slug)
        {
            var post = await _context.Posts
                .Where(p => p.Slug.Equals(slug) && p.IsPublished).AsNoTracking()
                .Include(p => p.Comments).ToListAsync();
            return post.DefaultIfEmpty(new Post()).FirstOrDefault();
        }

        public async Task<IEnumerable<Post>> GetPostsBySearchData(PostManagerSearch searchData)
        {
            var postList = await GetAllPosts();

            if (searchData == null) return postList;

            if (!postList.Any()) return postList;

            switch (searchData.PublishStatus)
            {
                case (int)PostManagerSearch.PublishStatusList.Any:
                    break;
                case (int)PostManagerSearch.PublishStatusList.Published:
                    postList = postList.Where(p => p.IsPublished);
                    break;
                case (int)PostManagerSearch.PublishStatusList.Unpublished:
                    postList = postList.Where(p => !p.IsPublished);
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(searchData.PostTitle))
            {
                postList = postList.Where(p => p.Title.Contains(searchData.PostTitle,
                    StringComparison.OrdinalIgnoreCase));
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
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteComment(string id)
        {
            try
            {
                var comment = await GetCommentById(id);
                if (string.IsNullOrEmpty(comment.Id))
                {
                    return false;
                }

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Comment> GetCommentById(string id)
        {
            var comment = await _context.Comments.Where(c => c.Id.Equals(id)).ToListAsync();
            return comment.DefaultIfEmpty(new Comment()).FirstOrDefault();
        }

        public async Task<bool> UpdatePost(PostEditViewModel postEditViewModel)
        {
            try
            {
                var post = await GetPostById(postEditViewModel.PostId);

                post.Title = postEditViewModel.Title;
                post.Content = postEditViewModel.Content;
                post.Description = postEditViewModel.Description;
                post.Slug = postEditViewModel.Slug;
                post.EditDate = DateTime.Now;
                post.IsPublished = postEditViewModel.Published;

                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /**
         * Used for checking if slug is unique when editing a post
         */
        public async Task<bool> CheckIfSlugIsUnique(string slug, string postId)
        {
            var postContainingSlug = await _context.Posts
                .Where(p => p.Slug.Equals(slug)
                            && !p.Id.Equals(postId)).ToListAsync();
            return postContainingSlug.Count == 0;
        }

        /**
         * Used for checking if slug is unique for new posts that do not have id
         */
        public async Task<bool> CheckIfSlugIsUnique(string slug)
        {
            var postContainingSlug = await _context.Posts
                .Where(p => p.Slug.Equals(slug)).ToListAsync();

            return postContainingSlug.Count == 0;
        }

        public async Task Save()
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
                Description = post.Description,
                Slug = post.Slug,
                IsPublished = post.Publish,
                PubDate = DateTime.Now,
                AuthorUserName = author.UserName,
                AuthorId = author.Id
            };
        }

    }
}