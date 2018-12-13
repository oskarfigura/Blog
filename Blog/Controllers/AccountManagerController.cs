using System.Security.Claims;
using System.Threading.Tasks;
using Blog.Areas.Identity.Data;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers
{
    [Authorize(Policy = "CanAccessAccountManager")]
    public class AccountManagerController : Controller
    {
        private const string TempDataOperationParam = "UserOperationResult";
        private const string ViewDataManagerMsgParam = "ManagerMessage";
        private const string ViewDataEditResult = "EditResult";
        private const string ViewDataDeleteResult = "DeleteResult";

        private readonly IUserRepo _userRepo;

        public AccountManagerController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<IActionResult> Index(AccountManagerViewModel accountManagerViewModel)
        {
            var accountSearchData = accountManagerViewModel.AccountSearch;

            //Get any result messages from CRUD operations on accounts
            if (TempData[TempDataOperationParam] != null)
            {
                ViewData[ViewDataManagerMsgParam] = TempData[TempDataOperationParam].ToString();
            }

            return View(new AccountManagerViewModel
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

            return View(new AccountViewModel
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

            return View(new AccountViewModel
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
                    ViewData[ViewDataEditResult] = "User updated successfully!";
                    var result = await _userRepo.UpdateUser(accountViewModel.UserAccount);
                    if (!result)
                    {
                        ViewData[ViewDataEditResult] = "Please try again.";
                    }

                    return View(accountViewModel);
                }

                ViewData[ViewDataEditResult] = "Email already exists, please enter a different email.";
                return View(accountViewModel);
            }
            ViewData[ViewDataEditResult] = "Please try again.";
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

            return View(new AccountDeleteViewModel
            {
                UserName = user.UserName
            });
        }

        // POST: AccountManager/Delete, process deleting account
        [Authorize(Policy = "CanDeleteUsers")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(AccountDeleteViewModel deleteViewModel)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userRepo.GetUserById(currentUserId);

            if (deleteViewModel.UserName.Equals(user.UserName))
            {
                ViewData[ViewDataDeleteResult] = "You cannot delete yourself from account manager! Please do it in profile settings.";
                return View(deleteViewModel);
            }

            var deleteResult = await _userRepo.DeleteUser(deleteViewModel.UserName);

            if (deleteResult)
            {
                TempData[TempDataOperationParam] = "User deleted successfully.";
                return RedirectToAction("Index");
            }

            ViewData[ViewDataDeleteResult] = "Unexpected error occurred! Please try again.";
            return View(deleteViewModel);
        }
    }
}