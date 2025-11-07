using Common.Entities;
using Example.MySql.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EfAutoMigration;
using Microsoft.EntityFrameworkCore;
using Example.PostgreSql.Seeders;

const string TABLE_NAME = "Users";

Console.WriteLine("Running Example.MySql on .NET 8 with EfAutoMigration...");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        var optionBuilder = new DbContextOptionsBuilder<MyDbContext>();
        var connString = "Server=localhost;Database=testdb;User=root;Password=password";
        // Register DbContext
        services.AddDbContext<MyDbContext>(options =>
            options.UseMySql(connString, ServerVersion.AutoDetect(connString)));

        // Enable automatic EF migrations & seeder process (optional) with a marker table
        services.AddEfAutoMigration<MyDbContext>(TABLE_NAME)
                .AddSeeders<MyDbContext>( //AddSeeder here is optional
                new UserSeeder());
    })
    .Build();

// Run migrations automatically at startup
await host.StartAsync();
Console.WriteLine("Database migrated");