using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Models;
using Microsoft.AspNetCore.Identity;

namespace Blog.Areas.Identity.Data
{
    public interface IUserRepo : IDisposable
    {
        Task<IEnumerable<AccountViewModel>> GetUsersBySearchData(AccountSearch searchModel);
        Task<IEnumerable<IdentityRole>> GetAllRoles();
//        Task<BlogUser> GetUserById(string userId);
//        void InsertUser(BlogUser user);
//        void DeleteUser(string userId);
//        void UpdateUser(BlogUser user);
        void Save();
    }
}
