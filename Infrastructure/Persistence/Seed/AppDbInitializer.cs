using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seed;

public class AppDbInitializer : IAppDbInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<AppRoles> _roleManager;

    public AppDbInitializer(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<AppRoles> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitializeAsync()
    {
        if (_context.Database.GetPendingMigrations().Any())
        {
            await _context.Database.MigrateAsync();
        }

        await SeedRolesAsync();

        await SeedAdminAsync();
    }

    private async Task SeedRolesAsync()
    {
        string[] roles =
        {
            AppRoles.Admin,
            AppRoles.PreAdmin,
            AppRoles.Teacher,
            AppRoles.Student
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new AppRoles
                {
                    Name = role,
                    Description = role
                });
            }
        }
    }

    private async Task SeedAdminAsync()
    {
        const string email = "admin@school.com";

        var admin = await _userManager.FindByEmailAsync(email);

        if (admin != null)
            return;

        admin = new ApplicationUser
        {
            FirstName = "System",
            LastName = "Administrator",
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            IsActive = true,
            ForceChangedPassword = true
        };

        var result = await _userManager.CreateAsync(
            admin,
            "Admin@123");

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(
                admin,
                AppRoles.Admin);
        }
    }
}
