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
        public async Task<IActionResult> Details(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return NotFound();
            }

            var user = await _userRepo.GetUserByUserName(userName);

            if (string.IsNullOrEmpty(user.UserName))
            {
                return NotFound();
            }

            return View(new AccountViewModel()
            {
                UserAccount = user
            });
        }

        // GET: AccountManager/Edit, view for editing account details 
        [Authorize(Policy = "CanEditUsers")]
        public async Task<IActionResult> Edit(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return NotFound();
            }

            var user = await _userRepo.GetUserByUserName(userName);
            var availableRoles = await _userRepo.GetAllRoles();

            if (string.IsNullOrEmpty(user.UserName))
            {
                return NotFound();
            }

            return View(new AccountViewModel()
            {
                UserAccount = user,
                AvailableIdentityRoles = availableRoles
            });
        }

        // POST: AccountManager/Edit, process a change of account details
        [Authorize(Policy = "CanEditUsers")]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AccountViewModel accountViewModel)
        {
            //Populate available list of roles after form post cleared the list
            accountViewModel.AvailableIdentityRoles = await _userRepo.GetAllRoles();

            if (accountViewModel.UserAccount != null)
            {
                var emailIsUnique = await _userRepo
                    .CheckIfEmailIsUnique(accountViewModel.UserAccount.Email,
                    accountViewModel.UserAccount.Id);

                if (emailIsUnique)
                {
                    ViewData["EditResult"] = "User updated successfully!";
                    var result = await _userRepo.UpdateUser(accountViewModel.UserAccount);
                    if (!result)
                    {
                        ViewData["EditResult"] = "Please try again.";
                    }

                    return View(accountViewModel);
                }

                ViewData["EditResult"] = "Email already exists, please enter a different email.";
                return View(accountViewModel);
            }
            ViewData["EditResult"] = "Please try again.";
            return View(accountViewModel);
        }

        // GET: AccountManager/Delete, Display delete account view
        [Authorize(Policy = "CanDeleteUsers")]
        public async Task<IActionResult> Delete(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return NotFound();
            }

            var user = await _userRepo.GetUserByUserName(userName);

            if (string.IsNullOrEmpty(user.UserName))
            {
                return NotFound();
            }

            AccountDeleteViewModel deleteViewModel = new AccountDeleteViewModel
            {
                UserName = user.UserName
            };
            return View(deleteViewModel);
        }

    }
}