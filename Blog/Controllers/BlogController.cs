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
    public class BlogController : Controller
    {

        private readonly IPostRepo _postRepo;

        public BlogController(IPostRepo postRepo)
        {
            _postRepo = postRepo;
        }

        public IActionResult Index()
        {
            //TODO HOME PAGE list all posts 
            return View();
        }

        [Route("/Blog/{slug?}")]
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

//            return View(new PostViewModel()
//            {
//                //TODO Set model
//            });
            return View();
        }
    }
}