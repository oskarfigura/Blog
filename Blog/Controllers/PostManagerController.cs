using System;
using System.Collections.Generic;
using System.Linq;
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

        public PostManagerController(IPostRepo postRepo)
        {
            _postRepo = postRepo;
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
                BlogPosts = await _postRepo.GetAllPosts(),
                SearchTitle = postSearchData
            });
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
            var result = await _postRepo.AddPost(createPostViewModel);

            if (result.Succeeded)
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

            var slugIsUnique = await _postRepo.CheckIfSlugIsUnique(postEditViewModel.Slug, postEditViewModel.PostId);

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

    }
}