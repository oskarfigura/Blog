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

        public UserRepo(BlogContext context, UserManager<BlogUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<BlogUser>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<BlogUser> GetUserById(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async void InsertUser(BlogUser user)
        {
            await _context.Users.AddAsync(user);
        }

        public async void DeleteUser(string userId)
        {
            BlogUser user = await _userManager.FindByIdAsync(userId);
            await _userManager.DeleteAsync(user);
        }

        public async void UpdateUser(BlogUser user)
        {
            await _userManager.UpdateAsync(user); 
        }

        /***
         * 
         * */

        public async void Save()
        {
           await _context.SaveChangesAsync();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {

                    if (this._userManager != null)
                    {
                        this._userManager.Dispose();
                    }
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
