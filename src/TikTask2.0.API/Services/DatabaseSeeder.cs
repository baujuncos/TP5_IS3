using Microsoft.EntityFrameworkCore;
using TikTask2._0.API.Data;
using TikTask2._0.API.Models;

namespace TikTask2._0.API.Services;

public static class DatabaseSeeder
{
    public static async Task SeedAdminUser(AppDbContext context)
    {
        // Check if admin user already exists
        if (await context.Users.AnyAsync(u => u.Role == "Admin"))
        {
            return;
        }

        // Create default admin user
        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@tiktask.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Admin"
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
        
        Console.WriteLine("Admin user created - Username: admin, Password: Admin123!");
    }
}
