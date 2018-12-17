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
        private const string TempDataOperationParam = "PostOperationResult";
        private const string MsgSomethingIsWrong = "Something went wrong. Please try again.";


        private readonly IPostRepo _postRepo;
        private readonly IUserRepo _userRepo;

        public BlogController(IPostRepo postRepo, IUserRepo userRepo)
        {
            _postRepo = postRepo;
            _userRepo = userRepo;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _postRepo.GetAllPublishedPosts();

            return View(new BlogViewModel()
            {
                BlogPosts = posts
            });
        }

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

        [Authorize(Policy = "CanAccessPostManager")]
        [ActionName("Post")]
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

            return View(CreatePostViewModel(post));
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
                Comments = post.Comments,
                Slug = post.Slug
            });
        }

        // POST: Blog/Delete, process deleting a post
        [Authorize(Policy = "CanDeletePosts")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string postId)
        {
            var deleteResult = await _postRepo.DeletePost(postId);

            TempData[TempDataOperationParam] = MsgSomethingIsWrong;
            if (!deleteResult) return RedirectToAction("Index", "PostManager");

            TempData[TempDataOperationParam] = "Post deleted successfully.";
            return RedirectToAction("Index", "PostManager");
        }

        // POST: Blog/AddComment, process adding a comment
        [Authorize(Policy = "CanComment")]
        [HttpPost, ActionName("AddComment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(string comment, string postId, string postSlug)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userRepo.GetUserById(currentUserId);
            var post = await _postRepo.GetPostById(postId);
            var result = await _postRepo.AddComment(BlogUtils.CreateComment(user, comment, post));

            if (result != null)
            {
                return Redirect(Url.Action("Post", new { slug = postSlug }) + "#comments");
            }

            return RedirectToAction("Post", new { slug = postSlug });
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
            return Redirect(Url.Action("Post", new { slug = postSlug }) + "#comments");
        }
    }
}