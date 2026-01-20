using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;       // Needed to read appsettings.json
using Microsoft.Extensions.DependencyInjection; // Needed for GetRequiredService
using VetClinic.Models;

namespace VetClinic.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            var userManager = service.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
            var config = service.GetRequiredService<IConfiguration>();

            // Create roles
            await CreateRoleAsync(roleManager, "Admin");
            await CreateRoleAsync(roleManager, "Doctor");
            await CreateRoleAsync(roleManager, "Client");

            // 3. Read Credentials from appsettings.json
            var adminEmail = config["SeedData:AdminEmail"];
            var adminPassword = config["SeedData:AdminPassword"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                throw new Exception("Error: Please run 'dotnet user-secrets set' for SeedData:AdminEmail and SeedData:AdminPassword, or add them to appsettings.json.");
            }

            // Create the Admin User
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Super",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    IsDarkMode = true
                };

                var result = await userManager.CreateAsync(newAdmin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }

        private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}