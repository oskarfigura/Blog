using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Areas.Identity.Data;
using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Blog.Controllers
{
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
    }
}