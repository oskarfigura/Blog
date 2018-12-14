using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blog.Areas.Identity.Data;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers
{
    [Authorize(Policy = "CanAccessPostManager")]
    public class PostManagerController : Controller
    {
        private const string TempDataOperationParam = "PostOperationResult";
        private const string ViewDataManagerMsgParam = "PostManagerMessage";
        private const string ViewDataAddPostResult = "AddPostResult";
        private const string ViewDataEditPostResult = "EditPostResult";
        private const string ViewDataDeletePostResult = "DeletePostResult";
        private const string MsgSomethingIsWrong = "Something went wrong. Please try again.";

        private readonly IPostRepo _postRepo;
        private readonly IUserRepo _userRepo;

        public PostManagerController(IPostRepo postRepo, IUserRepo userRepo)
        {
            _postRepo = postRepo;
            _userRepo = userRepo;
        }

        public async Task<IActionResult> Index(PostManagerViewModel postManagerView)
        {
            var postSearchData = postManagerView.SearchTitle;

            //Get any result messages from CRUD operations on posts
            if (TempData[TempDataOperationParam] != null)
            {
                ViewData[ViewDataManagerMsgParam] = TempData[TempDataOperationParam].ToString();
            }

            return View(new PostManagerViewModel
            {
                BlogPosts = await _postRepo.GetPostsBySearchData(postSearchData),
                SearchTitle = postSearchData
            });
        }

        // GET: PostManager/ViewPost, redirect to post view
        public IActionResult ViewPost(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            return RedirectToAction("Post", "Blog", new {slug});
        }

        // GET: PostManager/AddPost, view for adding a new post
        [Authorize(Policy = "CanCreatePosts")]
        public IActionResult AddPost()
        {
            return View(new PostCreateViewModel());
        }

        // POST: PostManager/AddPost, process adding a new post
        [Authorize(Policy = "CanCreatePosts")]
        [HttpPost, ActionName("AddPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPost(PostCreateViewModel createPostViewModel)
        {
            var slug = CreateSlug(createPostViewModel.Slug);

            if (string.IsNullOrEmpty(slug))
            {
                ViewData[ViewDataEditPostResult] =
                    "Slug contains reserved characters, please only use letters and spaces.";
                return View(createPostViewModel);
            }

            var slugIsUnique = await _postRepo.CheckIfSlugIsUnique(slug);

            if (!slugIsUnique)
            {
                ViewData[ViewDataAddPostResult] = "Slug already exists, please enter a different slug.";
                return View(createPostViewModel);
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userRepo.GetUserById(currentUserId);

            createPostViewModel.Slug = slug;
            var result = await _postRepo.AddPost(createPostViewModel, user);

            if (result)
            {
                TempData[TempDataOperationParam] = "Post created successfully!";
                return RedirectToAction("Index");
            }

            ViewData[ViewDataAddPostResult] = MsgSomethingIsWrong;
            return View(createPostViewModel);
        }

        // GET: PostManager/Edit, view for editing a post
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
                Slug = post.Slug,
                Published = post.IsPublished
            });
        }

        // POST: PostManager/Edit, process a change of post data
        [Authorize(Policy = "CanEditPosts")]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PostEditViewModel postEditViewModel)
        {
            if (string.IsNullOrWhiteSpace(postEditViewModel.PostId))
            {
                return NotFound();
            }

            var slug = CreateSlug(postEditViewModel.Slug);

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
                    ViewData[ViewDataEditPostResult] = "Unable to edit post. Please try again later.";
                }

                return View(postEditViewModel);
            }

            ViewData[ViewDataEditPostResult] = "Slug already exists, please enter a different slug.";
            return View(postEditViewModel);
        }

        // GET: PostManager/Delete, Display delete post view
        [Authorize(Policy = "CanDeletePosts")]
        public async Task<IActionResult> Delete(string postId)
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

            return View(new PostDeleteViewModel()
            {
                PostId = post.Id,
                Title = post.Title,
            });
        }

        // POST: PostManager/Delete, process deleting a post
        [Authorize(Policy = "CanDeletePosts")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(PostDeleteViewModel deleteViewModel)
        {
            var deleteResult = await _postRepo.DeletePost(deleteViewModel.PostId);

            if (deleteResult)
            {
                TempData[TempDataOperationParam] = "Post deleted successfully.";
                return RedirectToAction("Index");
            }

            ViewData[ViewDataDeletePostResult] = "Unexpected error occurred! Please try again.";
            return View(deleteViewModel);
        }

        /**
         * Formats user input slug into a valid slug that can be used in a url
         */
        public static string CreateSlug(string slug)
        {
            slug = slug.ToLowerInvariant().Replace(" ", "-");
            slug = RemoveDiacritics(slug);
            slug = RemoveReservedUrlCharacters(slug);
            slug = RemoveRepeatedHyphens(slug);

            return slug.ToLowerInvariant();
        }

        /**
         * Used to remove unwanted characters from slug
         */
        private static string RemoveReservedUrlCharacters(string text)
        {
            var reservedCharacters = new List<string>
            {
                "!", "#", "$", "&", "'", "(", ")", "*", ",", "/", ":", ";", "=", "?", "@", "[", "]", "\"", "%", ".",
                "<", ">", "\\", "^", "_", "'", "{", "}", "|", "~", "`", "+", "£", "¬"
            };

            foreach (var chr in reservedCharacters)
            {
                text = text.Replace(chr, "");
            }

            return text;
        }

        /**
         * Used to remove unwanted diacritics from slug
         */
        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /**
         * Removes repeated hyphens from text
         */
        private static string RemoveRepeatedHyphens(string text)
        {
            var initialTrim = Regex.Replace(text, "-+", "-");
            var finalTrim = initialTrim.Trim('-');
            return finalTrim;
        }
    }
}