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
            // 1. Get the Services (Security Guard, Role Manager, and Config Reader)
            var userManager = service.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
            var config = service.GetRequiredService<IConfiguration>();

            // 2. Create Roles (if they don't exist)
            await CreateRoleAsync(roleManager, "Admin");
            await CreateRoleAsync(roleManager, "Doctor");
            await CreateRoleAsync(roleManager, "Client");

            // 3. Read Credentials from appsettings.json
            var adminEmail = config["SeedData:AdminEmail"];
            var adminPassword = config["SeedData:AdminPassword"];

            // 4. SECURITY CHECK: Crash if credentials are missing
            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                throw new Exception("CRITICAL ERROR: SeedData:AdminEmail or SeedData:AdminPassword is missing from appsettings.json");
            }

            // 5. Create the Admin User
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

                // Create the user with the password from config
                var result = await userManager.CreateAsync(newAdmin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }

        // Helper method to create roles
        private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}