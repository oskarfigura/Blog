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
    public class UserRepo : IUserRepo, IDisposable
    {
        private readonly BlogContext _context;
        private readonly UserManager<BlogUser> _userManager;
        private bool _disposed = false;

        public UserRepo(BlogContext context, UserManager<BlogUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<User>> GetUsersBySearchData(AccountSearch searchModel)
        {
            var result = await GetUsers();

            if (searchModel == null) return result;

            if (!string.IsNullOrEmpty(searchModel.UserName))
            {
                result = result.Where(x => x.UserName.Contains(searchModel.UserName,
                    StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(searchModel.Role))
            {
                result = result.Where(x => x.Role.Contains(searchModel.Role,
                    StringComparison.OrdinalIgnoreCase));
            }

            return result;
        }

        public async Task<IEnumerable<IdentityRole>> GetAllRoles()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<User> GetUserById(string userId)
        {
            var user = new User();

            if (string.IsNullOrEmpty(userId)) return user;

            var blogUser = await GetUserByIdFromDb(userId);
            var role = await GetUsersRoleName(userId);

            user = GetAccountView(blogUser, role);
            return user;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            var user = new User();

            if (string.IsNullOrEmpty(email)) return user;

            try
            {
                var blogUser = await GetUserByEmailFromDb(email);
                var role = await GetUsersRoleName(blogUser.Id);
                user = GetAccountView(blogUser, role);
            }
            catch (Exception e)
            {
                return user;
            }

            return user;
        }

        



        //
        //        public async void InsertUser(BlogUser user)
        //        {
        //            await _context.Users.AddAsync(user);
        //        }
        //
        //        public async void DeleteUser(string userId)
        //        {
        //            var user = await _userManager.FindByIdAsync(userId);
        //            await _userManager.DeleteAsync(user);
        //        }
        //
        //        public async void UpdateUser(BlogUser user)
        //        {
        //            await _userManager.UpdateAsync(user); 
        //        }

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

        private async Task<IEnumerable<BlogUser>> GetBlogUsersFromDb()
        {
            return await _context.Users.ToListAsync();
        }

        private async Task<BlogUser> GetUserByIdFromDb(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        private async Task<BlogUser> GetUserByEmailFromDb(string userEmail)
        {
            return await _userManager.FindByEmailAsync(userEmail);
        }
        
        private async Task<IEnumerable<User>> GetUsers()
        {
            var users = await GetBlogUsersFromDb();
            var result = new List<User>();

            foreach (var user in users)
            {
                var role = await GetUsersRoleName(user.Id);
                result.Add(GetAccountView(user, role));
            }

            return result;
        }

        private async Task<string> GetUsersRoleName(string userId)
        {
            var usersRoleId = await GetUsersRoleId(userId);
            var usersIdentityRole = await GetRoleById(usersRoleId);

            return usersIdentityRole.Name;
        }

        private async Task<string> GetUsersRoleId(string userId)
        {
            var identityUserRole = await _context.UserRoles.Where(x => x.UserId.Equals(userId)).FirstAsync();
            return identityUserRole.RoleId;
        }

        private async Task<IdentityRole> GetRoleById(string roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            return role;
        }
    }
}
