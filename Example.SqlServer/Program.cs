using Common.Entities;
using EfAutoMigration;
using Example.PostgreSql.Seeders;
using Example.SqlServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

const string TABLE_NAME = "Users";

Console.WriteLine("Running Example.SqlServer on .NET 8 with EfAutoMigration...");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Register DbContext
        services.AddDbContext<MyDbContext>(options =>
                    options.UseSqlServer("Server=localhost;Database=TestDb;User Id=sa;Password=password;TrustServerCertificate=True"));

        // Enable automatic EF migrations & seeder process (optional) with a marker table
        services.AddEfAutoMigration<MyDbContext>(TABLE_NAME)
                .AddSeeders<UserSeeder, MyDbContext>();
    })
    .Build();

// Run migrations automatically at startup
await host.StartAsync();
Console.WriteLine("Database migrated");

await host.StopAsync();