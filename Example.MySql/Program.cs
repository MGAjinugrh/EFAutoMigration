using Common.Entities;
using Example.MySql.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EfAutoMigration;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Running Example.MySql on .NET 8 with EfAutoMigration...");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        var optionBuilder = new DbContextOptionsBuilder<MyDbContext>();
        var connString = "Server=localhost;Database=testdb;User=root;Password=password";
        // Register DbContext
        services.AddDbContext<MyDbContext>(options =>
            options.UseMySql(connString, ServerVersion.AutoDetect(connString)));

        // Enable auto-migration (with marker table 'user')
        services.AddEfAutoMigration<MyDbContext>("user");
    })
    .Build();

// Run migrations automatically at startup
await host.StartAsync();
Console.WriteLine("Database migrated");

// Example of initial user seeding
const string TEST_USER = "testuser";
using(var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

    if (!db.Users.Any(u => u.Username == TEST_USER))
    {
        db.Users.Add(new User
        {
            Username = TEST_USER,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
            CreatedAt = DateTime.UtcNow,
            CreatorId = 0
        });
        await db.SaveChangesAsync();
        Console.WriteLine("User seeded into database");
    }
}