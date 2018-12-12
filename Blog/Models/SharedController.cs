using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Models
{
    public class SharedController : Controller
    {
        public IActionResult Error()
        {
            return View(new ErrorViewModel()
            {
                RequestId = Response.StatusCode.ToString()
            });
        }
    }
}