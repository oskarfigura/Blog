using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blog.Areas.Identity.Data;
using Blog.Models;
using Blog.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        private const string ViewDataPostResultMsg = "PostResultMsg";
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
            //TODO Run this method to store posts which will be used for db seeding in the future
            //TODO StoreAllPostsInJsonFile();

            var postSearchData = postManagerView.SearchData;

            //Get any result messages from CRUD operations on posts
            if (TempData[TempDataOperationParam] != null)
            {
                ViewData[ViewDataManagerMsgParam] = TempData[TempDataOperationParam].ToString();
            }

            return View(new PostManagerViewModel
            {
                BlogPosts = await _postRepo.GetPostsBySearchData(postSearchData),
                SearchData = postSearchData
            });
        }

        [Route("/PostManager/ViewPost/{slug?}")]
        public async Task<IActionResult> Post(string slug)
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
                Comments = post.Comments
            });
        }

        //TODO Delete after testing is done, only used to save posts created in web, ready for db seeding
        private async void StoreAllPostsInJsonFile()
        {
            var posts = await _postRepo.GetAllPosts();

            var json = JsonConvert.SerializeObject(new
            {
                posts
            });

            const string path = @"Areas/Identity/Data/BlogPosts/posts.json";

            using (var file = new StreamWriter(path))
            {
                file.WriteLine(json);
            }
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
            var slug = BlogUtils.CreateSlug(createPostViewModel.Slug);

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
                Description = post.Description,
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

        // POST: PostManager/Delete, process deleting a post
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

        // POST: PostManager/Delete, process deleting a comment
        [Authorize(Policy = "CanDeleteComment")]
        [HttpPost, ActionName("DeleteComment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(string commentId, string postSlug)
        {
            if (string.IsNullOrEmpty(commentId))
            {
                return NotFound();
            }

            //var comment = await _postRepo.GetCommentById(commentId);

//            if (string.IsNullOrEmpty(comment.Id))
//            {
//                return NotFound();
//            }

            return RedirectToAction("Post", new {postSlug});
        }
    }
}