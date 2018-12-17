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
        private const string ViewDataManagerMsgParam = "AccManagerMessage";
        private const string ViewDataEditResult = "EditResult";
        private const string MsgUnexpectedError = "Something went wrong. Please try again.";
        private const string MsgUserUpdated = "User updated successfully!";
        private const string MsgUserDeleted = "User deleted successfully.";
        private const string MsgDuplicateEmail = "Email already exists, please enter a different email.";

        private const string MsgCannotDeleteYourself = "You cannot delete yourself from account manager! " +
                                                       "Please do it in your profile settings.";

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
                ViewData[ViewDataManagerMsgParam] =
                    TempData[TempDataOperationParam].ToString();
            }

            return View(new AccountManagerViewModel
            {
                Accounts = await _userRepo
                    .GetUsersBySearchData(accountManagerViewModel.AccountSearch),
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

            if (accountViewModel.UserAccount == null)
            {
                ViewData[ViewDataEditResult] = MsgUnexpectedError;
                return View(accountViewModel);
            }

            var emailIsUnique = await _userRepo
                .CheckIfEmailIsUnique(accountViewModel.UserAccount.Email,
                    accountViewModel.UserAccount.Id);

            if (!emailIsUnique)
            {
                ViewData[ViewDataEditResult] = MsgDuplicateEmail;
                return View(accountViewModel);
            }

            ViewData[ViewDataEditResult] = MsgUserUpdated;
            var result = await _userRepo.UpdateUser(accountViewModel.UserAccount);
            if (!result) ViewData[ViewDataEditResult] = MsgUnexpectedError;

            return View(accountViewModel);
        }

        // POST: AccountManager/Delete, process deleting account
        [Authorize(Policy = "CanDeleteUsers")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string userName)
        {
            var user = await GetLoggedInUser();

            if (userName.Equals(user.UserName))
            {
                TempData[TempDataOperationParam] = MsgCannotDeleteYourself;
                return RedirectToAction("Index");
            }

            var deleteResult = await _userRepo.DeleteUser(userName);

            if (deleteResult)
            {
                TempData[TempDataOperationParam] = MsgUserDeleted;
                return RedirectToAction("Index");
            }

            TempData[TempDataOperationParam] = MsgUnexpectedError;
            return RedirectToAction("Index");
        }

        private async Task<User> GetLoggedInUser()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userRepo.GetUserById(currentUserId);
            return user;
        }
    }
}