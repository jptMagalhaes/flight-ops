using FlightOps.Entities;
using Microsoft.AspNetCore.Identity;

namespace FlightOps.Data;

public static class IdentitySeeder
{
    public const string OperatorRole = "Operator";
    public const string ViewerRole = "Viewer";

    public static async Task SeedAsync(IServiceProvider services)
    {
        RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        UserManager<ApplicationUser> userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (string role in new[] { OperatorRole, ViewerRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await EnsureUserAsync(userManager, "operator@flightops.demo", "Operator123!", OperatorRole);
        await EnsureUserAsync(userManager, "viewer@flightops.demo", "Viewer123!", ViewerRole);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager, string email, string password, string role)
    {
        ApplicationUser? existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return;

        ApplicationUser user = new() { UserName = email, Email = email, EmailConfirmed = true };
        IdentityResult result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
    }
}
