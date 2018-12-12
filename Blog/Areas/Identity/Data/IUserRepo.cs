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
        Task<IEnumerable<User>> GetUsersBySearchData(AccountSearch searchModel);
        Task<IEnumerable<IdentityRole>> GetAllRoles();
        Task<User> GetUserById(string userId);
        Task<User> GetUserByUserName(string userName);
        Task<bool> CheckIfEmailIsUnique(string email, string userId);
        Task<bool> UpdateUser(User updatedUser);
        //        void DeleteUser(string userId);
        void Save();
    }
}
