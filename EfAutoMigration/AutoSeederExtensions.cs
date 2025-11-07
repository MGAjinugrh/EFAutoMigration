using EfAutoMigration.Interfaces;
using EfAutoMigration.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
    /// Registers one or more <see cref="ISeeder{TContext}"/> implementations
    /// to automatically run after migrations are applied.
    /// 
    /// <para>
    /// Usage example:
    /// <code>
    /// services.AddEfAutoMigration&lt;MyDbContext&gt;("Users")
    ///         .AddSeeders&lt;MyDbContext&gt;(
    ///             new UserSeeder(),
    ///             new RoleSeeder()
    ///         );
    /// </code>
    /// </para>
    /// 
    /// <para>
    /// This method is optional — you can safely omit it
    /// if your app does not require initial data seeding.
    /// </para>
    /// </summary>
    /// <typeparam name="TContext">The EF Core DbContext type.</typeparam>
    /// <param name="services">The dependency injection service container.</param>
    /// <param name="seeders">One or more seeder instances implementing <see cref="ISeeder{TContext}"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for fluent chaining.</returns>
    public static IServiceCollection AddSeeders<TContext>(
            this IServiceCollection services,
            params ISeeder<TContext>[] seeders
        ) where TContext : DbContext
    {
        // Register the seeders
        foreach (var seeder in seeders)
        {
            services.AddSingleton<ISeeder<TContext>>(seeder);
        }

        // Register a hosted service that runs seeding logic after migration
        services.AddHostedService<EfSeederHostedService<TContext>>();

        return services;
    }
}
