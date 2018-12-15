using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Blog.Areas.Identity.Data
{
    public class DbInitializer
    {
        public static readonly string[] AdminUsers = {"Member1"};

        public static readonly string[] EditorUsers = { };

        public static readonly string[] FollowerUsers =
        {
            "Customer1", "Customer2",
            "Customer3", "Customer4", "Customer5"
        };

        private const string DefaultPassword = "Password123!";
        private const string DefaultEmail = "@email.com";

        private const string AdminRole = "Admin";
        private const string EditorRole = "Editor";
        private const string FollowerRole = "Follower";

        private const string CreatingCommentsClaim = "CanComment";
        private const string DeletingCommentsClaim = "CanDeleteComments";
        private const string AccessingAccountManagerClaim = "CanAccessAccountManager";
        private const string AccessingPostManagerClaim = "CanAccessPostManager";
        private const string EditingUsersClaim = "CanEditUsers";
        private const string DeletingUsersClaim = "CanDeleteUsers";
        private const string ChangingUserPermissionsClaim = "CanChangeUserPermissions";
        private const string EditingPostsClaim = "CanEditPosts";
        private const string CreatingPostsClaim = "CanCreatePosts";
        private const string DeletingPostsClaim = "CanDeletePosts";

        //Seed database with default data 
        public async Task SeedDb(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var context = serviceProvider.GetRequiredService<BlogContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<BlogUser>>();
            await SeedRoles(roleManager);
            await SeedUsers(userManager);
            //SeedBlogPosts -> 
            //ReadBlogPosts();    //TODO Reading blog posts from file
        }

        //Seed database with roles
        private async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = {AdminRole, EditorRole, FollowerRole};

            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);

                if (!roleExists)
                {
                    var role = new IdentityRole
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper()
                    };

                    var roleResult = await roleManager.CreateAsync(role);
                    if (roleResult.Succeeded)
                    {
                        await SeedRoleClaims(roleManager, role);
                    }
                }
            }
        }

        //Seed the db with role claims
        private async Task SeedRoleClaims(RoleManager<IdentityRole> roleManager, IdentityRole role)
        {
            var roleName = role.Name;

            if (roleName.Equals(AdminRole))
            {
                string[] claims =
                {
                    CreatingCommentsClaim, DeletingCommentsClaim, EditingPostsClaim,
                    CreatingPostsClaim, DeletingPostsClaim, EditingUsersClaim, AccessingPostManagerClaim,
                    DeletingUsersClaim, AccessingAccountManagerClaim, ChangingUserPermissionsClaim
                };

                await AddClaimsToRole(claims, roleManager, role);
            }
            else if (roleName.Equals(EditorRole))
            {
                string[] claims =
                {
                    CreatingCommentsClaim, DeletingCommentsClaim, EditingPostsClaim,
                    CreatingPostsClaim, DeletingPostsClaim, AccessingPostManagerClaim
                };

                await AddClaimsToRole(claims, roleManager, role);
            }
            else if (roleName.Equals(FollowerRole))
            {
                string[] claims = {CreatingCommentsClaim};
                await AddClaimsToRole(claims, roleManager, role);
            }
        }

        //Add claims to a role
        private async Task AddClaimsToRole(string[] claims, RoleManager<IdentityRole> roleManager, IdentityRole role)
        {
            var roleClaimList = (await roleManager.GetClaimsAsync(role)).Select(p => p.Type);

            foreach (var claim in claims)
            {
                if (!roleClaimList.Contains(claim))
                {
                    await roleManager.AddClaimAsync(role, new Claim(claim, "true"));
                }
            }
        }

        //Seeds the db with users
        private async Task SeedUsers(UserManager<BlogUser> userManager)
        {
            string[] defaultUsers = AdminUsers.Concat(EditorUsers).Concat(FollowerUsers).ToArray();

            //Create users and seed to database
            foreach (var username in defaultUsers)
            {
                var blogUser = await userManager.FindByEmailAsync(username + DefaultEmail);

                //Check if this user already exists
                if (blogUser == null)
                {
                    var newUser = new BlogUser
                    {
                        Name = username,
                        DisplayName = username,
                        UserName = username + DefaultEmail,
                        Email = username + DefaultEmail
                    };

                    var result = await userManager.CreateAsync(newUser, DefaultPassword);

                    //Add roles and claims to users
                    if (result.Succeeded)
                    {
                        await SeedUserRoles(newUser, userManager);
                    }
                }
            }
        }

        //Seeds the db with user roles
        private async Task SeedUserRoles(BlogUser newUser, UserManager<BlogUser> userManager)
        {
            var username = newUser.UserName;

            if (AdminUsers.Any(x => (x + DefaultEmail).Contains(username)))
            {
                await userManager.AddToRoleAsync(newUser, AdminRole);
            }
            else if (EditorUsers.Any(x => (x + DefaultEmail).Contains(username)))
            {
                await userManager.AddToRoleAsync(newUser, EditorRole);
            }
            else if (FollowerUsers.Any(x => (x + DefaultEmail).Contains(username)))
            {
                await userManager.AddToRoleAsync(newUser, FollowerRole);
            }
        }

        //        private async Task SeedBlogPosts()
        //        {
        //          readBlogPosts() --> Seed the db using this data
        //        }

        private void ReadBlogPosts()
        {
            var data = "";
            const string path = @"Areas/Identity/Data/BlogPosts/posts.json";

            using (var file = new StreamReader(path))
            {
                data = file.ReadToEnd();
            }

            var result = JsonConvert.DeserializeObject<PostJsonResult>(data);

            foreach (var post in result.Posts)
            {
                /*
                 *
                 * var user = await userManager.FindByNameAsync(post.AuthorUserName);
                 * post.AuthorId = user.Id;
                 */
                //Check user for each post and get userId
                //Seed db with post
            }
        }
    }
}