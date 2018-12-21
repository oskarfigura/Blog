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
        private const string MsgUnexpectedError = "Something went wrong. Please try again.";
        private const string MsgUserDeleted = "User deleted successfully.";
        private const string MsgDuplicateEmail = "Email already exists, please enter a different email.";

        private const string MsgCannotDeleteYourself = "You cannot delete yourself from account manager! " +
                                                       "Please do it in your profile settings.";

        private const string ModelStateErrorMsgKey = "errorMsg";

        private readonly IUserRepo _userRepo;

        public AccountManagerController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Index(AccountManagerViewModel accountManagerViewModel)
        {
            var accountSearchData = accountManagerViewModel.AccountSearch;
            var message = GetResultMsg(accountManagerViewModel.ResultStatus);

            return View(await CreateAccountManagerViewModel(accountSearchData, message));
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

            if (ModelState.IsValid)
            {
                var emailIsUnique = await _userRepo
                    .CheckIfEmailIsUnique(accountViewModel.UserAccount.Email,
                        accountViewModel.UserAccount.Id);

                if (!emailIsUnique)
                {
                    ModelState.AddModelError(ModelStateErrorMsgKey, MsgDuplicateEmail);
                    return View(accountViewModel);
                }

                var result = await _userRepo.UpdateUser(accountViewModel.UserAccount);
                if (result)
                {
                    return RedirectToAction("Details",
                        new {userName = accountViewModel.UserAccount.UserName});
                }
            }

            ModelState.AddModelError(ModelStateErrorMsgKey, MsgUnexpectedError);
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
                return RedirectToAction("Index",
                    new
                    {
                        ResultStatus = (int)AccountManagerViewModel
                            .ResultStatusList.CannotDeleteYourself
                    });
            }

            var deleteResult = await _userRepo.DeleteUser(userName);

            if (deleteResult)
            {
                return RedirectToAction("Index",
                    new
                    {
                        ResultStatus = (int)AccountManagerViewModel
                            .ResultStatusList.UserDeleted
                    });
            }

            return RedirectToAction("Index",
                new
                {
                    ResultStatus = (int)AccountManagerViewModel
                        .ResultStatusList.UnexpectedError
                });
        }

        private async Task<User> GetLoggedInUser()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userRepo.GetUserById(currentUserId);
            return user;
        }

        private async Task<AccountManagerViewModel> CreateAccountManagerViewModel(
            AccountSearch accountSearchData, string message)
        {
            return new AccountManagerViewModel
            {
                Accounts = await _userRepo.GetUsersBySearchData(accountSearchData),
                AvailableIdentityRoles = await _userRepo.GetAllRoles(),
                AccountSearch = accountSearchData,
                ResultMsg = message
            };
        }

        private async Task<AccountManagerViewModel> CreateAccountManagerViewModel()
        {
            var searchData = new AccountSearch();
            return new AccountManagerViewModel
            {
                Accounts = await _userRepo.GetUsersBySearchData(searchData),
                AvailableIdentityRoles = await _userRepo.GetAllRoles(),
                AccountSearch = searchData,
                ResultMsg = ""
            };
        }

        /**
         * Decodes which info message should be shown to user based on operation result
         */
        private static string GetResultMsg(int statusValue)
        {
            var message = "";

            switch (statusValue)
            {
                case (int) AccountManagerViewModel.ResultStatusList.UserDeleted:
                    message = MsgUserDeleted;
                    break;
                case (int) AccountManagerViewModel.ResultStatusList.UnexpectedError:
                    message = MsgUnexpectedError;
                    break;
                case (int) AccountManagerViewModel.ResultStatusList.CannotDeleteYourself:
                    message = MsgCannotDeleteYourself;
                    break;
                default:
                    break;
            }

            return message;
        }
    }
}