using Common.Entities;
using Example.SqlServer.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EfAutoMigration;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Running Example.SqlServer on .NET 8 with EfAutoMigration...");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Register DbContext
        services.AddDbContext<MyDbContext>(options =>
                    options.UseSqlServer("Server=localhost;Database=TestDb;User Id=sa;Password=password;TrustServerCertificate=True"));

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