using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Areas.Identity.Data
{
    public interface IUserRepo : IDisposable
    {
        Task<IEnumerable<BlogUser>> GetUsers();
        Task<BlogUser> GetUserById(string userId);
        void InsertUser(BlogUser user);
        void DeleteUser(string userId);
        void UpdateUser(BlogUser user);
        void Save();
    }
}
