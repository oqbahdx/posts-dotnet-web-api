using Microsoft.EntityFrameworkCore;
using Posts.Data;
using Posts.Helpers;
using Posts.Models.Entities;

namespace Posts.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureCreatedAsync();

        if (await context.Users.AnyAsync())
        {
            return;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "demo@example.com",
            PasswordHash = PasswordHasher.Hash("Demo@123"),
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);

        var posts = new[]
        {
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Getting Started with ASP.NET Core",
                Description = "ASP.NET Core is a cross-platform, high-performance framework for building modern cloud-based apps.",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UserId = user.Id
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Entity Framework Core Best Practices",
                Description = "Learn how to optimize your EF Core queries, manage migrations, and follow repository patterns effectively.",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UserId = user.Id
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "JWT Authentication in .NET",
                Description = "A comprehensive guide to implementing JWT-based authentication and authorization in ASP.NET Core Web APIs.",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UserId = user.Id
            }
        };

        context.Posts.AddRange(posts);
        await context.SaveChangesAsync();
    }
}
