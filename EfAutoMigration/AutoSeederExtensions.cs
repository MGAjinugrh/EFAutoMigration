using EfAutoMigration.Interfaces;
using EfAutoMigration.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace EfAutoMigration;

/// <summary>
/// Provides extension methods to register one or more data seeders
/// that will automatically execute after EF Core migrations have completed.
/// 
/// This feature is optional — call <see cref="AutoMigrationExtensions.AddEfAutoMigration{TContext}(IServiceCollection, string[])"/>
/// alone if you only need automatic schema migrations.
/// </summary>
public static class AutoSeederExtensions
{
    /// <summary>
    /// Registers a seeder type to be resolved via Dependency Injection.
    /// This allows the Seeder to inject services like ILogger, IConfiguration, etc.
    /// </summary>
    /// <typeparam name="TSeeder">The class implementing ISeeder</typeparam>
    /// <typeparam name="TContext">The DbContext</typeparam>
    public static IServiceCollection AddSeeders<TSeeder, TContext>(this IServiceCollection services)
            where TContext : DbContext
            where TSeeder : class, ISeeder<TContext>
    {
        // 1. Register the seeder itself
        services.AddScoped<ISeeder<TContext>, TSeeder>();

        // 2. Ensure the hosted service runner is registered (only once)
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, EfSeederHostedService<TContext>>());

        return services;
    }
}
