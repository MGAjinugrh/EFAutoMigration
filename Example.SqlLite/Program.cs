using Common.Entities;
using EfAutoMigration;
using Example.SqlLite.Data;
using Example.SqlLite.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

const string TABLE_NAME = "Users";

Console.WriteLine("Running Example.Sqlite on .NET 8 with EfAutoMigration...");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Use SQLite database (stored locally)
        services.AddDbContext<MyDbContext>(options =>
            options.UseSqlite("Data Source=./example_sqlite.db"));

        // Enable automatic EF migrations & seeder process (optional) with a marker table
        services.AddEfAutoMigration<MyDbContext>(TABLE_NAME)
                .AddSeeders<UserSeeder, MyDbContext>();
    })
    .Build();

// Run migrations automatically at startup
await host.StartAsync();
Console.WriteLine("Database migrated successfully.");

await host.StopAsync();
Console.WriteLine("Application finished.");