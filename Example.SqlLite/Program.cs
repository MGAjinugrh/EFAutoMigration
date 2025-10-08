using Common.Entities;
using EfAutoMigration;
using Example.SqlLite.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Running Example.Sqlite on .NET 8 with EfAutoMigration...");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Use SQLite database (stored locally)
        services.AddDbContext<MyDbContext>(options =>
            options.UseSqlite("Data Source=./example_sqlite.db"));

        // Enable automatic EF migrations with a marker table
        services.AddEfAutoMigration<MyDbContext>("Users");
    })
    .Build();

// Run migrations automatically at startup
await host.StartAsync();
Console.WriteLine("Database migrated successfully.");

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
        Console.WriteLine("User seeded into database.");
    }
}

await host.StopAsync();
Console.WriteLine("Application finished.");