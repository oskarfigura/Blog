using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Areas.Identity.Data;
using Blog.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blog
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<BlogContext>(options => options.UseSqlServer(
                Configuration.GetConnectionString("BlogContextConnection")));


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanComment", policy => policy.RequireClaim("CanComment", "true"));
                options.AddPolicy("CanDeleteComment", policy => policy.RequireClaim("CanDeleteComment", "true"));
                options.AddPolicy("CanEditPosts", policy => policy.RequireClaim("CanEditPosts", "true"));
                options.AddPolicy("CanCreatePosts", policy => policy.RequireClaim("CanCreatePosts", "true"));
                options.AddPolicy("CanDeletePosts", policy => policy.RequireClaim("CanDeletePosts", "true"));
                options.AddPolicy("CanEditUsers", policy => policy.RequireClaim("CanEditUsers", "true"));
                options.AddPolicy("CanDeleteUsers", policy => policy.RequireClaim("CanDeleteUsers", "true"));
                options.AddPolicy("CanChangeUserPermissions", policy => policy.RequireClaim("CanChangeUserPermissions", "true"));
                options.AddPolicy("CanAccessUserManager", policy => policy.RequireClaim("CanAccessUserManager", "true"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Blog/Error");
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Blog}/{action=Index}/{id?}");
            });
        }
    }
}
