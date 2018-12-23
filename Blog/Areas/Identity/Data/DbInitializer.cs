using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Blog.Areas.Identity.Data
{
    public class DbInitializer
    {
        public static readonly string[] AdminUsers = {"Member1"};

        public static readonly string[] EditorUsers = { "Editor1", "Editor2" };

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

        private readonly UserManager<BlogUser> _userManager;
        private static RoleManager<IdentityRole> _roleManager;
        private readonly BlogContext _context;

        public DbInitializer(IServiceProvider serviceProvider)
        {
            _context = serviceProvider.GetRequiredService<BlogContext>();
            _roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            _userManager = serviceProvider.GetRequiredService<UserManager<BlogUser>>();
        }

        //Seed database with default data 
        public async Task SeedDb()
        {
            await SeedRoles();
            await SeedUsers();
            await SeedBlogPosts();
        }

        //Seed database with roles
        private async Task SeedRoles()
        {
            string[] roleNames = {AdminRole, EditorRole, FollowerRole};

            foreach (var roleName in roleNames)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);

                if (!roleExists)
                {
                    var role = new IdentityRole
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper()
                    };

                    var roleResult = await _roleManager.CreateAsync(role);
                    if (roleResult.Succeeded)
                    {
                        await SeedRoleClaims(role);
                    }
                }
            }
        }

        //Seed the db with role claims
        private async Task SeedRoleClaims(IdentityRole role)
        {
            var roleName = role.Name;

            if (roleName.Equals(AdminRole))
            {
                await SeedAdminRoleClaims(role);
            }
            else if (roleName.Equals(EditorRole))
            {
                await SeedEditorRoleClaims(role);
            }
            else if (roleName.Equals(FollowerRole))
            {
                await SeedFollowerRoleClaims(role);
            }
        }

        //Seed admin role with admin claims
        private async Task SeedAdminRoleClaims(IdentityRole role)
        {
            string[] claims =
            {
                CreatingCommentsClaim, DeletingCommentsClaim, EditingPostsClaim,
                CreatingPostsClaim, DeletingPostsClaim, EditingUsersClaim, AccessingPostManagerClaim,
                DeletingUsersClaim, AccessingAccountManagerClaim, ChangingUserPermissionsClaim
            };

            await AddClaimsToRole(claims, role);
        }

        //Seed editor role with editor claims
        private async Task SeedEditorRoleClaims(IdentityRole role)
        {
            string[] claims =
            {
                CreatingCommentsClaim, DeletingCommentsClaim, EditingPostsClaim,
                CreatingPostsClaim, DeletingPostsClaim, AccessingPostManagerClaim
            };

            await AddClaimsToRole(claims, role);
        }

        //Seed follower role with follower claims
        private async Task SeedFollowerRoleClaims(IdentityRole role)
        {
            string[] claims = { CreatingCommentsClaim };
            await AddClaimsToRole(claims, role);
        }

        //Add claims to a role
        private async Task AddClaimsToRole(string[] claims, IdentityRole role)
        {
            var roleClaimList = (await _roleManager.GetClaimsAsync(role)).Select(p => p.Type);

            foreach (var claim in claims)
            {
                if (!roleClaimList.Contains(claim))
                {
                    await _roleManager.AddClaimAsync(role, new Claim(claim, "true"));
                }
            }
        }

        //Seeds the db with users
        private async Task SeedUsers()
        {
            string[] defaultUsers = AdminUsers.Concat(EditorUsers).Concat(FollowerUsers).ToArray();

            //Create users and seed to database
            foreach (var username in defaultUsers)
            {
                var blogUser = await FindUser(username + DefaultEmail);

                //Check if this user already exists
                if (blogUser == null)
                {
                    var newUser = CreateUser(username);
                    var result = await _userManager.CreateAsync(newUser, DefaultPassword);

                    //Add roles and claims to users
                    if (result.Succeeded)
                    {
                        await SeedUserRoles(newUser);
                    }
                }
            }
        }

        //Seeds the db with user roles
        private async Task SeedUserRoles(BlogUser newUser)
        {
            var username = newUser.UserName;

            if (AdminUsers.Any(x => (x + DefaultEmail).Contains(username)))
            {
                await _userManager.AddToRoleAsync(newUser, AdminRole);
            }
            else if (EditorUsers.Any(x => (x + DefaultEmail).Contains(username)))
            {
                await _userManager.AddToRoleAsync(newUser, EditorRole);
            }
            else if (FollowerUsers.Any(x => (x + DefaultEmail).Contains(username)))
            {
                await _userManager.AddToRoleAsync(newUser, FollowerRole);
            }
        }

        //Seeds the db with example blog posts from file
        private async Task SeedBlogPosts()
        {
            var postList = ReadBlogPostsFromFile();
            foreach (var post in postList)
            {
                await SeedBlogPost(post);
            }
        }

        //Seeds the db with example blog post
        private async Task SeedBlogPost(Post post)
        {
            var user = await FindUser(post.AuthorUserName);
            post.AuthorId = user.Id;

            var result = await FindPost(post.Id);

            if (result == null)
            {
                await _context.Posts.AddAsync(post);
                await _context.SaveChangesAsync();
            }
        }

        //Finds post by id
        private async Task<Post> FindPost(string id)
        {
            return await _context.Posts.FindAsync(id);
        }

        //Finds user by email since email is used as username
        private async Task<BlogUser> FindUser(string email)
        {
           return await _userManager.FindByEmailAsync(email);
        }

        //Reads example blog posts from a file
        private static List<Post> ReadBlogPostsFromFile()
        {
            var data = "";
            const string path = @"Areas/Identity/Data/BlogPosts/posts.json";

            if (File.Exists(path))
            {
                using (var file = new StreamReader(path))
                {
                    data = file.ReadToEnd();
                }
            }

            var result = JsonConvert.DeserializeObject<PostJsonResult>(data);

            return result.Posts;
        }

        //Creates a user profile from username
        private static BlogUser CreateUser(string username)
        {
            return new BlogUser()
            {
                Name = username,
                DisplayName = username,
                UserName = username + DefaultEmail,
                Email = username + DefaultEmail
            };
        }
    }
}