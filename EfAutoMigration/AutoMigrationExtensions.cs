using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EfAutoMigration;

/// <summary>
/// Provides extension methods for registering automatic
/// Entity Framework Core migrations at application startup.
/// 
/// This extension integrates with the .NET Generic Host model
/// and runs migrations via <see cref="IHostedService"/>,
/// ensuring databases are created and schema is updated
/// without requiring manual <c>Database.Migrate()</c> calls.
/// </summary>
public static class AutoMigrationExtensions
{
    /// <summary>
    /// Registers automatic EF migrations at application startup.
    /// Runs via <see cref="IHostedService"/> so it works with any application
    /// using the .NET Generic Host model:
    /// ASP.NET Core, console apps, worker services, etc.
    /// </summary>
    /// <typeparam name="TContext">Your DbContext type</typeparam>
    /// <param name="services">DI container</param>
    /// <param name="markerTables">Optional: marker tables to check if schema exists</param>
    public static IServiceCollection AddEfAutoMigration<TContext>(
        this IServiceCollection services,
        params string[] markerTables)
        where TContext : DbContext
    {
        services.AddSingleton<IHostedService>(
            sp => new AutoMigrationHostedService<TContext>(sp, markerTables));

        return services;
    }


    /* ---------- nested private classes ---------- */

    private sealed class AutoMigrationHostedService<TContext> : IHostedService
        where TContext : DbContext
    {
        private readonly IServiceProvider _services;
        private readonly string[] _markers;

        public AutoMigrationHostedService(IServiceProvider services, string[] markers)
        {
            _services = services;
            _markers = markers ?? Array.Empty<string>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TContext>();
            RunMigrations(db, _markers);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    /* ---------- shared migration logic ---------- */

    private static void RunMigrations<TContext>(TContext db, string[] markers) where TContext : DbContext
    {
        try
        {
            // 1. Ensure database exists
            if (!db.Database.CanConnect())
            {
                var provider = db.Database.ProviderName ?? string.Empty;

                // SQLite creates the file automatically on open/migrate, so explicit Create() is often unnecessary or handled differently
                if (!ContainsIgnoreCase(provider,"sqlite"))
                {
                    var creator = db.GetService<IRelationalDatabaseCreator>();
                    try
                    {
                        creator.Create();
                    }
                    catch
                    {
                        // 2. Race Condition Handler:
                        // If Create() fails (e.g. "Database already exists"), check CanConnect() one more time.
                        // If we can connect now, it means the DB was created successfully by another process/retry.
                        if (!db.Database.CanConnect())
                        {
                            throw; // Real error (e.g. permission denied, wrong password), rethrow it.
                        }
                        // Otherwise, swallow the exception and proceed. The goal "DB exists" is met.
                    }
                }
            }

            // 3. Marker Table Check (Schema Detection)
            if (markers.Length > 0 && db.Database.CanConnect())
            {
                var markerList = string.Join(",", markers.Select(t => $"'{t}'"));
                var provider = db.Database.ProviderName ?? string.Empty;
                string sql = provider switch
                {
                    var p when p.Contains("Npgsql") =>
                        $"SELECT 1 FROM pg_tables WHERE tablename IN ({markerList}) LIMIT 1",

                    var p when p.Contains("MySql") || p.Contains("MariaDb") =>
                        $"SELECT 1 FROM information_schema.tables WHERE table_name IN ({markerList}) LIMIT 1",

                    var p when p.Contains("SqlServer") =>
                        $"SELECT 1 FROM sys.tables WHERE name IN ({markerList})",

                    var p when p.Contains("Sqlite") =>
                        $"SELECT 1 FROM sqlite_master WHERE type='table' AND name IN ({markerList}) LIMIT 1",

                    _ =>
                        $"SELECT 1 FROM information_schema.tables WHERE table_name IN ({markerList}) LIMIT 1"
                };

                try { _ = db.Database.ExecuteSqlRaw(sql) > 0; }
                catch { /* ignore schema detection errors */ }
            }

            // 4. Run migrations
            db.Database.Migrate();
        }
        catch
        {
            throw; // bubble up so host app can handle
        }
    }

    /// <summary>
    /// A remedy since .netstandard2.0 didn't support string.Contains(object, StringComparison.OrdinalIgnoreCase)
    /// </summary>
    /// <param name="source"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool ContainsIgnoreCase(string source, string value)
    => source?.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
}
