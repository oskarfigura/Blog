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
        private const string TempDataOperationParam = "BlogOperationResult";
        private const string ViewDataEditPostResult = "EditPostResult";
        private const string ViewDataPostDeleteResultMsg = "PostDeleteResult";

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

            //Get any result messages from CRUD operations on posts
            if (TempData[TempDataOperationParam] != null)
            {
                ViewData[ViewDataPostDeleteResultMsg] = TempData[TempDataOperationParam].ToString();
            }

            return View(new BlogViewModel()
            {
                BlogPosts = posts
            });
        }

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

            if (string.IsNullOrEmpty(post.Content))
            {
                return NotFound();
            }

            var formattedContent = BlogUtils.FormatPostContent(post.Content);

            return View(new PostViewModel()
            {
                Id = post.Id,
                Title = post.Title,
                Content = formattedContent,
                PubDate = post.PubDate,
                Comments = post.Comments,
                Slug = post.Slug
            });
        }
        // GET: Blog/Edit, view for editing a post
        [Authorize(Policy = "CanEditPosts")]
        public async Task<IActionResult> Edit(string postId)
        {
            if (string.IsNullOrEmpty(postId))
            {
                return NotFound();
            }

            var post = await _postRepo.GetPostById(postId);

            if (string.IsNullOrEmpty(post.Id))
            {
                return NotFound();
            }

            return View(new PostEditViewModel()
            {
                PostId = post.Id,
                Title = post.Title,
                Content = post.Content,
                Description = post.Description,
                Slug = post.Slug,
                Published = post.IsPublished
            });
        }

        // POST: Blog/Edit, process a change of post data
        [Authorize(Policy = "CanEditPosts")]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PostEditViewModel postEditViewModel)
        {
            if (string.IsNullOrWhiteSpace(postEditViewModel.PostId))
            {
                return NotFound();
            }

            var slug = BlogUtils.CreateSlug(postEditViewModel.Slug);

            if (string.IsNullOrEmpty(slug))
            {
                ViewData[ViewDataEditPostResult] =
                    "Slug contains reserved characters, please only use letters and spaces.";
                return View(postEditViewModel);
            }

            var slugIsUnique = await _postRepo.CheckIfSlugIsUnique(slug, postEditViewModel.PostId);

            if (slugIsUnique)
            {
                var updateResult = await _postRepo.UpdatePost(postEditViewModel);

                if (!updateResult)
                {
                    ViewData[ViewDataEditPostResult] = "Unable to edit post. Please try again.";
                }
                ViewData[ViewDataEditPostResult] = "Post updated successfully.";
                return View(postEditViewModel);
            }

            ViewData[ViewDataEditPostResult] = "Slug already exists, please enter a different slug.";
            return View(postEditViewModel);
        }

        // POST: Blog/Delete, process deleting a post
        [Authorize(Policy = "CanDeletePosts")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string postId)
        {
            var deleteResult = await _postRepo.DeletePost(postId);

            if (!deleteResult) return RedirectToAction("Index");

            TempData[TempDataOperationParam] = "Post deleted successfully.";
            return RedirectToAction("Index");
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

            var result = await _postRepo.DeleteComment(commentId);

            return Redirect(Url.Action("Post", new { slug = postSlug }) + "#comments");
        }
    }
}