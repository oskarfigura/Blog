using Blog.Areas.Identity.Data;
using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Areas.Identity.Data
{
    public class UserRepo : IUserRepo
    {
        private readonly BlogContext _context;
        private readonly UserManager<BlogUser> _userManager;
        private bool _disposed;

        public UserRepo(BlogContext context, UserManager<BlogUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /**
         * Find users using search data 
         */
        public async Task<ICollection<User>> GetUsersBySearchData(AccountSearch searchModel)
        {
            var result = await GetUsers();

            if (searchModel == null) return result.ToList();

            if (!string.IsNullOrEmpty(searchModel.UserName))
            {
                result = result.Where(x => x.UserName.Contains(searchModel.UserName,
                    StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.Role))
            {
                result = result.Where(x => x.Role.Contains(searchModel.Role,
                    StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return result.ToList();
        }

        /**
         * Return all available user roles
         */
        public async Task<ICollection<IdentityRole>> GetAllRoles()
        {
            return await _context.Roles.ToListAsync();
        }

        /**
         * Find user by user id
         */
        public async Task<User> GetUserById(string userId)
        {
            var user = new User();

            if (string.IsNullOrEmpty(userId)) return user;

            var blogUser = await GetUserByIdFromDb(userId);
            var role = await GetUsersRoleName(userId);

            user = GetAccountView(blogUser, role);
            return user;
        }

        /**
         * Find user by user name
         */
        public async Task<User> GetUserByUserName(string userName)
        {
            var user = new User();

            if (string.IsNullOrEmpty(userName)) return user;

            try
            {
                var blogUser = await GetUserByUserNameFromDb(userName);
                var role = await GetUsersRoleName(blogUser.Id);
                user = GetAccountView(blogUser, role);
                return user;
            }
            catch (Exception)
            {
                return user;
            }
        }

        /**
         * Update user including personal data and role
         */
        public async Task<bool> UpdateUser(User updatedUser)
        {
            var oldUser = await _userManager.FindByIdAsync(updatedUser.Id);

            if (oldUser == null) return false;

            var dataUpdateResult = await UpdateUserData(updatedUser, oldUser);
            var roleUpdateResult = await UpdateUserRole(updatedUser, oldUser);

            return dataUpdateResult && roleUpdateResult;
        }

        /**
         * Check email to ensure it doesn't already exist 
         */
        public async Task<bool> CheckIfEmailIsUnique(string email, string userId)
        {
            var user = await GetUserByEmail(email);

            if (user == null || string.IsNullOrEmpty(user.Email)) return true;

            //Check if found email belongs to the user
            return user.Id == userId;
        }

        /**
         * Delete user from db
         */
        public async Task<bool> DeleteUser(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return false;

            var user = await GetUserByUserNameFromDb(userName);
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async void Save()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _userManager?.Dispose();
                    _context.Dispose();
                }
            }

            this._disposed = true;
        }

        /**
         * Update user personal data
         */
        private async Task<bool> UpdateUserData(User updatedUser, BlogUser oldUser)
        {
            oldUser.Name = updatedUser.Name;
            oldUser.DisplayName = updatedUser.DisplayName;
            oldUser.Email = updatedUser.Email;
            oldUser.PhoneNumber = updatedUser.PhoneNumber;

            var result = await _userManager.UpdateAsync(oldUser);
            return result.Succeeded;
        }

        /**
         * Replace user old role with new role
         */
        private async Task<bool> UpdateUserRole(User updatedUser, BlogUser oldUser)
        {
            var usersRoles = await _userManager.GetRolesAsync(oldUser);
            var userRole = usersRoles.First();

            if (userRole.Equals(updatedUser.Role))
            {
                return true;
            }

            var roleRemovingResult = await RemoveUserFromRoles(oldUser, usersRoles);
            var roleAddingResult = await AddUserToRole(oldUser, updatedUser.Role);

            return roleRemovingResult && roleAddingResult;
        }

        /**
         * Removes all of user roles
         */
        private async Task<bool> RemoveUserFromRoles(BlogUser user, IList<string> usersRoles)
        {
            var result = await _userManager.RemoveFromRolesAsync(user, usersRoles);
            return result.Succeeded;
        }

        /**
         * Adds user to a role
         */
        private async Task<bool> AddUserToRole(BlogUser user, string role)
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        /**
         * Returns an object that includes the role of a user
         */
        private static User GetAccountView(BlogUser blogUser, string role)
        {
            var user = new User()
            {
                Id = blogUser.Id,
                Name = blogUser.Name,
                DisplayName = blogUser.DisplayName,
                UserName = blogUser.UserName,
                Email = blogUser.Email,
                EmailConfirmed = blogUser.EmailConfirmed,
                PhoneNumber = blogUser.PhoneNumber,
                PhoneNumberConfirmed = blogUser.PhoneNumberConfirmed,
                Role = role
            };

            return user;
        }

        /**
         * Returns list of all users
         */
        private async Task<ICollection<User>> GetUsers()
        {
            var users = await GetBlogUsersFromDb();
            var result = new List<User>();

            foreach (var user in users)
            {
                var role = await GetUsersRoleName(user.Id);
                result.Add(GetAccountView(user, role));
            }

            result = result.OrderBy(user => user.UserName).ToList();
            return result;
        }

        /**
         * Find user by email
         */
        private async Task<User> GetUserByEmail(string email)
        {
            var user = new User();

            if (string.IsNullOrEmpty(email)) return user;

            try
            {
                var blogUser = await GetUserByEmailFromDb(email);
                var role = await GetUsersRoleName(blogUser.Id);
                user = GetAccountView(blogUser, role);
                return user;
            }
            catch (Exception)
            {
                return user;
            }
        }

        /**
         * Returns a list of all users from the database
         */
        private async Task<ICollection<BlogUser>> GetBlogUsersFromDb()
        {
            return await _context.Users.ToListAsync();
        }

        /**
         * Returns a user with the specified id 
         */
        private async Task<BlogUser> GetUserByIdFromDb(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        /**
         * Returns a user with the specified email address 
         */
        private async Task<BlogUser> GetUserByEmailFromDb(string userEmail)
        {
            return await _userManager.FindByEmailAsync(userEmail);
        }

        /**
         * Returns a user with the specified user name
         */
        private async Task<BlogUser> GetUserByUserNameFromDb(string userName)
        {
            return await _context.Users.Where(x => x.UserName.Equals(userName)).FirstAsync();
        }

        /**
         * Returns name of the user role
         */
        private async Task<string> GetUsersRoleName(string userId)
        {
            var usersRoleId = await GetUsersRoleId(userId);
            var usersIdentityRole = await GetRoleById(usersRoleId);

            return usersIdentityRole.Name;
        }

        /**
         * Returns id of the user role
         */
        private async Task<string> GetUsersRoleId(string userId)
        {
            var identityUserRole = await _context.UserRoles.Where(x => x.UserId.Equals(userId)).FirstAsync();
            return identityUserRole.RoleId;
        }

        /**
         * Find role using id
         */
        private async Task<IdentityRole> GetRoleById(string roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            return role;
        }
    }
}