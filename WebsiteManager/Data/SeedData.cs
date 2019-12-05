using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteManager.Models;

namespace WebsiteManager.Data
{
    public class SeedData
    {
        /// <summary>
        /// https://docs.microsoft.com/en-us/aspnet/core/security/authorization/secure-data?view=aspnetcore-2.2
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="testUserPw"></param>
        /// <returns></returns>
		public static async Task Initialize(IServiceProvider serviceProvider, string testUserPw)
        {
            using (var context = new ApplicationDbContext(
               serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                //context.Database.EnsureCreated();

                // For sample purposes seed both with the same password.
                // Password is set with the following:
                // dotnet user-secrets set SeedUserPW <pw>
                // The admin user can do anything

                if (context.Users.Any())
                {
                    return;
                }
                else
                {
                    var danielId = await EnsureUser(serviceProvider, testUserPw, "daniel@bpapos.com", "daniel@bpapos.com");
                    await EnsureRole(serviceProvider, danielId, "admin");

                    var paulId = await EnsureUser(serviceProvider, testUserPw, "paul@bpapos.com", "paul@bpapos.com");
                    await EnsureRole(serviceProvider, paulId, "admin");

                    var joeId = await EnsureUser(serviceProvider, testUserPw, "joe@bpapos.com", "joe@bpapos.com");
                    await EnsureRole(serviceProvider, joeId, "admin");

                    var bssCompany = await EnsureUser(serviceProvider, testUserPw, "BSS");
                }
            }
        }

        private static async Task<string> EnsureUser(IServiceProvider serviceProvider,
                                                 string testUserPw, string userName, string email = null)
        {
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    user = new ApplicationUser { UserName = userName, Email = email };
                }
                else
                {
                    user = new ApplicationUser { UserName = userName };
                }

                user.EmailConfirmed = true;
                await userManager.CreateAsync(user, testUserPw);
            }

            return user.Id;
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider,
                                                                      string uid, string role)
        {
            try
            {
                IdentityResult IR = null;
                var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

                if (roleManager == null)
                {
                    throw new Exception("roleManager null");
                }

                if (!await roleManager.RoleExistsAsync(role))
                {
                    IR = await roleManager.CreateAsync(new IdentityRole(role));
                }

                var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

                var user = await userManager.FindByIdAsync(uid.ToString());

                IR = await userManager.AddToRoleAsync(user, role);

                return IR;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                throw;
            }

        }
    }
}
