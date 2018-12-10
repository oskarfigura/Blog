using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Blog.Areas.Identity.Data
{
    public class DbInitializer
    {
        public static readonly string[] adminUsers = { "Member1" };

        public static readonly string[] editorUsers = { };

        public static readonly string[] followerUsers = { "Customer1", "Customer2",
            "Customer3", "Customer4", "Customer5" };

        private const string defaultPassword = "Password123!";
        private const string defaultEmail = "@email.com";

        private const string adminRole = "Admin";
        private const string editorRole = "Editor";
        private const string followerRole = "Follower";

        private const string creatingCommentsClaim = "CanComment";
        private const string deletingCommentsClaim = "CanDeleteComments";
        private const string accessingAccountManagerClaim = "CanAccessAccountManager";
        private const string editingUsersClaim = "CanEditUsers";
        private const string deletingUsersClaim = "CanDeleteUsers";
        private const string changingUserPermissionsClaim = "CanChangeUserPermissions";
        private const string editingPostsClaim = "CanEditUsers";
        private const string creatingPostsClaim = "CanCreatePosts";
        private const string deletingPostsClaim = "CanDeletePosts";


        //Seed database with default data 
        public async Task SeedDb(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var context = serviceProvider.GetRequiredService<BlogContext>();

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<BlogUser>>();
            await SeedRoles(roleManager);
            await SeedUsers(userManager);
        }

        //Seed database with roles
        private async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { adminRole, editorRole, followerRole };

            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);

                if (!roleExists)
                {
                    IdentityRole role = new IdentityRole
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper()
                    };

                    var roleResult = await roleManager.CreateAsync(role);
                    await SeedRoleClaims(roleManager, role);
                }
            }
        }

        //Seed the db with role claims
        private async Task SeedRoleClaims(RoleManager<IdentityRole> roleManager, IdentityRole role)
        {
            var roleName = role.Name;

            if (roleName.Equals(adminRole))
            {
                string[] claims = { creatingCommentsClaim, deletingCommentsClaim, editingPostsClaim,
                                    creatingPostsClaim, deletingPostsClaim, editingUsersClaim,
                                     deletingUsersClaim, accessingAccountManagerClaim, changingUserPermissionsClaim };

                await AddClaimsToRole(claims, roleManager, role);
            }
            else if (roleName.Equals(editorRole))
            {
                string[] claims = { creatingCommentsClaim, deletingCommentsClaim, editingPostsClaim,
                                    creatingPostsClaim, deletingPostsClaim, editingUsersClaim, deletingUsersClaim,
                                    accessingAccountManagerClaim, changingUserPermissionsClaim };

                await AddClaimsToRole(claims, roleManager, role);
            }
            else if (roleName.Equals(followerRole))
            {
                string[] claims = { creatingCommentsClaim };
                await AddClaimsToRole(claims, roleManager, role);
            }
        }

        //Add claims to a role
        private async Task AddClaimsToRole(string[] claims, RoleManager<IdentityRole> roleManager, IdentityRole role)
        {
            var roleName = role.Name;
            var roleClaimList = (await roleManager.GetClaimsAsync(role)).Select(p => p.Type);

            foreach (string claim in claims)
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
            string[] defaultUsers = adminUsers.Concat(editorUsers).Concat(followerUsers).ToArray();

            //Create users and seed to database
            foreach (var username in defaultUsers)
            {
                var blogUser = await userManager.FindByEmailAsync(username + defaultEmail);

                //Check if this user already exists
                if (blogUser == null)
                {
                    BlogUser newUser = new BlogUser
                    {
                        Name = username,
                        DisplayName = username,
                        UserName = username + defaultEmail,
                        Email = username + defaultEmail
                    };

                    var result = await userManager.CreateAsync(newUser, defaultPassword);

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
            var email = defaultEmail;

            if (adminUsers.Any(x => (x + email).Contains(username)))
            {
                await userManager.AddToRoleAsync(newUser, adminRole);
            }
            else if (editorUsers.Any(x => (x + email).Contains(username)))
            {
                await userManager.AddToRoleAsync(newUser, editorRole);
            }
            else if (followerUsers.Any(x => (x + email).Contains(username)))
            {
                await userManager.AddToRoleAsync(newUser, followerRole);
            }
        }

        //private async Task SeedBlogPosts()
        //{

        //}
    }
}
