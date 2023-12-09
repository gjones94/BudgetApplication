using BudgetApp.Areas.Identity;
using BudgetApp.Areas.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace BudgetApp.Data
{
    public class SeedData
    {
        public static async Task<bool> InitializeAsync(IServiceProvider serviceProvider)
        {
            //Make sure that you are making this method "Initialize()" return a task so that the calling thread can wait on this task
            UserManager<User> UserManager  = serviceProvider.GetRequiredService<UserManager<User>>();
            ApplicationDbContext dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (dbContext == null)
            {
                return false;
            }

            /* Add Roles */
            foreach (Role role in RoleProvider.GetAllRoles())
            {
                if (!dbContext.Roles.Any(r => r.Name == role.Name))
                {
                    dbContext.Roles.Add(role);
                }
            }

            dbContext.SaveChanges();

            //Add Users
            if (!dbContext.Users.Any(r => r.UserName == "***********"))
            {
                var user = GetDefaultAdminUser();

                // Non-sensitive password
                IdentityResult result = await UserManager.CreateAsync(user, "*********");

                if(result.Succeeded)
                {
                    result = await UserManager.AddToRoleAsync(user, RoleName.Admin);
                }

                return result.Succeeded;
            }

            return true; //no other errors
        }

        private static User GetDefaultAdminUser()
        {
            return new User()
            {
                Id = Guid.NewGuid(),
                FirstName = "First Name",
                LastName = "Last Name",
                Email = "******************",
                BirthDate = DateTime.MinValue,
                NormalizedEmail = "******************",
                UserName = "*******************",
                NormalizedUserName = "****************",
                PhoneNumber = "**************",
                EmailConfirmed = true,
                ApprovedByAdmin = true,
                PhoneNumberConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };
        }
    }
}
