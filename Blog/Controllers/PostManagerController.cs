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
    [Authorize(Policy = "CanAccessPostManager")]
    public class PostManagerController : Controller
    {
        private const string MsgSomethingIsWrong = "Something went wrong. Please try again";
        private const string MsgInvalidSlug = "Slug contains reserved characters, please only use letters and spaces.";
        private const string MsgDuplicateSlug = "Slug already exists, please enter a different slug.";
        private const string MsgPostDeleted = "Post deleted successfully. ";
        private const string MsgSearchNoResult = "Your search returned no result. ";
        private const string MsgBlogIsEmpty = "There are currently no posts on this blog. ";
        private const string ModelStateErrorMsgKey = "errorMsg";

        private readonly IPostRepo _postRepo;
        private readonly IUserRepo _userRepo;

        public PostManagerController(IPostRepo postRepo, IUserRepo userRepo)
        {
            _postRepo = postRepo;
            _userRepo = userRepo;
        }

        // GET: PostManager, post manager home view with a list of all posts
        public async Task<IActionResult> Index(PostManagerViewModel postManagerView)
        {
            return View(await CreatePostManagerViewModel(
                postManagerView.SearchData,
                postManagerView.PostDeleted));
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
            if (ModelState.IsValid)
            {
                var slug = BlogUtils.CreateSlug(createPostViewModel.Slug);

                if (string.IsNullOrEmpty(slug))
                {
                    ModelState.AddModelError(ModelStateErrorMsgKey, MsgInvalidSlug);
                    return View(createPostViewModel);
                }

                var slugIsUnique = await _postRepo.CheckIfSlugIsUnique(slug);

                if (!slugIsUnique)
                {
                    ModelState.AddModelError(ModelStateErrorMsgKey, MsgDuplicateSlug);
                    return View(createPostViewModel);
                }

                createPostViewModel.Slug = slug;
                var user = await GetLoggedInUser();
                var result = await _postRepo.AddPost(createPostViewModel, user);

                if (result)
                {
                    return Redirect(Url.Action("AnyPost", "Blog", new {slug}));
                }
            }

            ModelState.AddModelError(ModelStateErrorMsgKey, MsgSomethingIsWrong);
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

            return View(CreatePostEditViewModel(post));
        }

        // POST: PostManager/Edit, process a change of post data
        [Authorize(Policy = "CanEditPosts")]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PostEditViewModel postEditViewModel)
        {
            if (ModelState.IsValid)
            {
                var slug = BlogUtils.CreateSlug(postEditViewModel.Slug);
                
                if (string.IsNullOrEmpty(slug))
                {
                    ModelState.AddModelError(ModelStateErrorMsgKey, MsgInvalidSlug);
                    return View(postEditViewModel);
                }

                var slugIsUnique = await _postRepo.CheckIfSlugIsUnique(slug, postEditViewModel.PostId);

                if (!slugIsUnique)
                {
                    ModelState.AddModelError(ModelStateErrorMsgKey, MsgDuplicateSlug);
                    return View(postEditViewModel);
                }
                postEditViewModel.Slug = slug;
                var updateResult = await _postRepo.UpdatePost(postEditViewModel);
                if (updateResult) return Redirect(Url.Action("AnyPost", "Blog", new {slug}));
            }

            ModelState.AddModelError(ModelStateErrorMsgKey, MsgSomethingIsWrong);
            return View(postEditViewModel);
        }

        // POST: PostManager/Delete, process deleting a post
        [Authorize(Policy = "CanDeletePosts")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string postId)
        {
            var deleteResult = await _postRepo.DeletePost(postId);
            return RedirectToAction("Index", new {PostDeleted = deleteResult});
        }

        private async Task<User> GetLoggedInUser()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userRepo.GetUserById(currentUserId);
            return user;
        }

        private async Task<PostManagerViewModel> CreatePostManagerViewModel(
            PostManagerSearch searchData, bool postDeleted)
        {
            var posts = await GetBlogPosts(searchData);
            var resultMsg = GetDeleteMsg(postDeleted);

            resultMsg = GetSearchResultMsg(resultMsg, searchData, posts.Any());

            return new PostManagerViewModel
            {
                BlogPosts = posts,
                SearchData = searchData,
                ResultMsg = resultMsg
            };
        }

        private async Task<ICollection<Post>> GetBlogPosts(PostManagerSearch searchData)
        {
            return await _postRepo.GetPostsBySearchData(searchData);
        }

        private static PostEditViewModel CreatePostEditViewModel(Post post)
        {
            return (new PostEditViewModel()
            {
                PostId = post.Id,
                Title = post.Title,
                Content = post.Content,
                Description = post.Description,
                Slug = post.Slug,
                Published = post.IsPublished
            });
        }

        private static string GetDeleteMsg(bool deleteResult)
        {
            return deleteResult ? MsgPostDeleted : "";
        }

        private static string GetSearchResultMsg(string currentMessage,
            PostManagerSearch searchData, bool anyPosts)
        {
            if (anyPosts) return currentMessage;
            if (searchData != null)
            {
                currentMessage = currentMessage + MsgSearchNoResult;
            }
            else
            {
                currentMessage = currentMessage + MsgBlogIsEmpty;
            }

            return currentMessage;
        }
    }
}