
using Microsoft.AspNetCore.Identity;

namespace AcademicScheduler.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Demo accounts must never be seeded in production.
        if (!environment.IsDevelopment())
            return;

        var roleManager = services
            .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager = services
            .GetRequiredService<UserManager<IdentityUser>>();

        string[] roles = { "Administrator", "Viewer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create role: {role}");
                }
            }
        }

        await CreateUserAsync(
            userManager,
            configuration["DemoUsers:AdminEmail"],
            configuration["DemoUsers:AdminPassword"],
            "Administrator");

        await CreateUserAsync(
            userManager,
            configuration["DemoUsers:ViewerEmail"],
            configuration["DemoUsers:ViewerPassword"],
            "Viewer");
    }

    private static async Task CreateUserAsync(
        UserManager<IdentityUser> userManager,
        string? email,
        string? password,
        string role)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                $"Missing demo credentials for {role}.");
        }

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(
                user, password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var result = await userManager.AddToRoleAsync(
                user, role);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign {role} role.");
            }
        }
    }
}
