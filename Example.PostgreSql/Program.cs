using Common.Entities;
using EfAutoMigration;
using Example.PostgreSql.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Running Example.PostgreSQL on .NET 8 with EfAutoMigration...");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Register DbContext
        services.AddDbContext<MyDbContext>(options =>
            options.UseNpgsql("Host=localhost;Database=testdb;Username=postgres;Password=postgres"));

        // Enable auto-migration (with marker table 'user')
        services.AddEfAutoMigration<MyDbContext>("user");
    })
    .Build();

// Run migrations automatically at startup
await host.StartAsync();
Console.WriteLine("Database migrated");

// Example seeding
const string TEST_USER = "testuser";
using (var scope = host.Services.CreateScope())
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

await host.StopAsync();