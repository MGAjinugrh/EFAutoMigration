using Common.Entities;
using EfAutoMigration.Interfaces;
using Example.PostgreSql.Data;

namespace Example.PostgreSql.Seeders;

public class UserSeeder : ISeeder<MyDbContext>
{
    public async Task SeedAsync(MyDbContext context)
    {
        if (!context.Users.Any())
        {
            context.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // obviously hash in real use
                CreatedAt = DateTime.UtcNow,
                CreatorId = 0
            });

            await context.SaveChangesAsync();
        }
    }
}
