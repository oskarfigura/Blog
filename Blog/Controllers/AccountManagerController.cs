using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Blog.Areas.Identity.Data;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Blog.Controllers
{
    [Authorize(Policy = "CanAccessAccountManager")]
    public class AccountManagerController : Controller
    {
        private readonly IUserRepo _userRepo;

        public AccountManagerController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<IActionResult> Index(AccountManagerViewModel accountManagerViewModel)
        {
            var accountSearchData = accountManagerViewModel.AccountSearch;

            return View(new AccountManagerViewModel()
            {
                Accounts = await _userRepo.GetUsersBySearchData(accountManagerViewModel.AccountSearch),
                AvailableIdentityRoles = await _userRepo.GetAllRoles(),
                AccountSearch = accountSearchData
            });
        }

        // GET: AccountManager/Details, view with account details 
        public async Task<IActionResult> Details(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return NotFound();
            }

            var user = await _userRepo.GetUserByEmail(email);

            if (string.IsNullOrEmpty(user.Email))
            {
                return NotFound();
            }

            return View(new AccountViewModel()
            {
                UserAccount = user
            });
        }


    }
}