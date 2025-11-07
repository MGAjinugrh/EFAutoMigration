using Common.Entities;
using EfAutoMigration;
using Example.PostgreSql.Data;
using Example.PostgreSql.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

const string TABLE_NAME = "Users";

Console.WriteLine("Running Example.PostgreSQL on .NET 8 with EfAutoMigration...");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Register DbContext
        services.AddDbContext<MyDbContext>(options =>
            options.UseNpgsql("Host=localhost;Database=testdb;Username=postgres;Password=postgres"));

        // Enable automatic EF migrations & seeder process (optional) with a marker table
        services.AddEfAutoMigration<MyDbContext>(TABLE_NAME)
                .AddSeeders<MyDbContext>( //AddSeeder here is optional
                new UserSeeder());
    })
    .Build();

// Run migrations automatically at startup
await host.StartAsync();
Console.WriteLine("Database migrated");

await host.StopAsync();