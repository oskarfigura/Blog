using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Blog.Areas.Identity.Data;
using Blog.Models;
using Blog.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers
{
    public class BlogController : Controller
    {
        private const string CommentsSection = "#comments";
        private const int TitleHomePageCharLimit = 30;
        private const string TruncateSymbol = "...";

        private readonly IPostRepo _postRepo;
        private readonly IUserRepo _userRepo;

        public BlogController(IPostRepo postRepo, IUserRepo userRepo)
        {
            _postRepo = postRepo;
            _userRepo = userRepo;
        }

        // GET: Blog/, view with all published posts
        public async Task<IActionResult> Index()
        {
            var posts = await _postRepo.GetAllPublishedPosts();

            return View(CreateBlogViewModel(posts.ToList()));
        }

        // GET: Blog/Post, view with post from slug (only published)
        [ActionName("Post")]
        [Route("/Blog/Post/{slug?}")]
        public async Task<IActionResult> Post(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var post = await _postRepo.GetPublishedPostBySlug(slug);

            if (string.IsNullOrEmpty(post.Id))
            {
                return NotFound();
            }

            return View(CreatePostViewModel(post));
        }

        // GET: Blog/AnyPost, view with post from slug (published or unpublished)
        [Authorize(Policy = "CanAccessPostManager")]
        [ActionName("AnyPost")]
        [Route("/Blog/AnyPost/{slug?}")]
        public async Task<IActionResult> AnyPost(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var post = await _postRepo.GetPostBySlug(slug);

            if (string.IsNullOrEmpty(post.Id))
            {
                return NotFound();
            }

            return View("Post", CreatePostViewModel(post));
        }

        // POST: Blog/Delete, process deleting a post
        [Authorize(Policy = "CanDeletePosts")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string postId)
        {
            var deleteResult = await _postRepo.DeletePost(postId);
            return RedirectToAction("Index", "PostManager",
                new {PostDeleted = deleteResult});
        }

        // POST: Blog/AddComment, process adding a comment
        [Authorize(Policy = "CanComment")]
        [HttpPost, ActionName("AddComment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(string comment,
            string postId, string postSlug)
        {
            var user = await GetLoggedInUser();
            var post = await _postRepo.GetPostById(postId);
            await _postRepo.AddComment(BlogUtils.CreateComment(user, comment, post));

            if (post.IsPublished)
            {
                return Redirect(Url.Action("Post",
                                    new {slug = postSlug}) + CommentsSection);
            }

            return Redirect(Url.Action("AnyPost",
                                new {slug = postSlug}) + CommentsSection);
        }

        // POST: Blog/Delete, process deleting a comment
        [Authorize(Policy = "CanDeleteComments")]
        [HttpPost, ActionName("DeleteComment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(string commentId, string postSlug)
        {
            if (string.IsNullOrEmpty(commentId))
            {
                return NotFound();
            }

            await _postRepo.DeleteComment(commentId);
            return Redirect(Url.Action("Post",
                                new {slug = postSlug}) + CommentsSection);
        }

        private static PostViewModel CreatePostViewModel(Post post)
        {
            post.Content = BlogUtils.FormatPostContent(post.Content);

            return (new PostViewModel()
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                PubDate = post.PubDate,
                Comments = post.Comments.OrderByDescending(c => c.PubDate),
                Slug = post.Slug
            });
        }

        private static BlogViewModel CreateBlogViewModel(IList<Post> posts)
        {
            foreach (var post in posts)
            {
                post.Title = TruncatePostTitle(post.Title);
            }

            return (new BlogViewModel()
            {
                BlogPosts = posts.OrderByDescending(p => p.PubDate)
            });
        }

        private static string TruncatePostTitle(string title)
        {
            if (string.IsNullOrEmpty(title) ||
                title.Length < TitleHomePageCharLimit) return title;
            title = title.Substring(0, TitleHomePageCharLimit);
            return title + TruncateSymbol;
        }

        private async Task<User> GetLoggedInUser()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userRepo.GetUserById(currentUserId);
            return user;
        }
    }
}