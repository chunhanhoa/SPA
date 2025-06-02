using Microsoft.AspNetCore.Identity;

namespace QL_Spa.Services
{
    public class RoleInitializer
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RoleInitializer> _logger;

        public RoleInitializer(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager,
            ILogger<RoleInitializer> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            // Create roles if they don't exist
            string[] roleNames = { "Admin", "User", "Manager" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Create the role
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Created role {roleName}");
                    }
                    else
                    {
                        _logger.LogError($"Failed to create role {roleName}");
                    }
                }
            }
        }

        // Optional: create an admin user if none exists
        public async Task CreateAdminUserAsync(string adminUsername, string adminEmail, string adminPassword)
        {
            var adminUser = await _userManager.FindByNameAsync(adminUsername);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminUsername,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation($"Created admin user {adminUsername}");
                }
                else
                {
                    _logger.LogError($"Failed to create admin user {adminUsername}");
                }
            }
            else if (!(await _userManager.IsInRoleAsync(adminUser, "Admin")))
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                _logger.LogInformation($"Added admin role to existing user {adminUsername}");
            }
        }
    }
}
