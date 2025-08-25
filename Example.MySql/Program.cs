using BCrypt.Net;
using Common.Entities;
using Example.MySql.Data.Factories;

Console.WriteLine("Running Example.MySql on .NET 8...");

using var context = new MyDbContextFactory().CreateDbContext(args);

// Trigger migration auto-applied
await context.Database.EnsureCreatedAsync();

Console.WriteLine("Database ready.");

// Example of initial user seeding
const string TEST_USER = "testuser";
if (!context.Users.Any(row => row.Username == TEST_USER))
{
    context.Users.Add(new User
    {
        Username = TEST_USER,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
        CreatedAt = DateTime.UtcNow,
    });
    await context.SaveChangesAsync();

    Console.WriteLine("User inserted successfully!");
}