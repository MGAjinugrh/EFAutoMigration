using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EfAutoMigration
{
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
                using (var scope = _services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<TContext>();
                    RunMigrations(db, _markers);
                }
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }

        /* ---------- shared migration logic ---------- */

        private static void RunMigrations<TContext>(TContext db, string[] markers) where TContext : DbContext
        {
            try
            {
                // Ensure database exists first
                if (!db.Database.CanConnect())
                {
                    var creator = db.GetService<IRelationalDatabaseCreator>();
                    creator.Create(); // creates database if missing
                }

                // Check if schema exists (optional, marker-based)
                bool schemaExists = false;
                var provider = db.Database.ProviderName ?? string.Empty;

                if (markers.Length > 0 && db.Database.CanConnect())
                {
                    var markerList = string.Join(",", markers.Select(t => "'" + t + "'"));
                    string sql;

                    if (provider.Contains("Npgsql"))
                        sql = $"SELECT 1 FROM pg_tables WHERE tablename IN ({markerList}) LIMIT 1";
                    else if (provider.Contains("MySql") || provider.Contains("MariaDb"))
                        sql = $"SELECT 1 FROM information_schema.tables WHERE table_name IN ({markerList}) LIMIT 1";
                    else
                        sql = $"SELECT 1 FROM information_schema.tables WHERE table_name IN ({markerList}) LIMIT 1";

                    try { schemaExists = db.Database.ExecuteSqlRaw(sql) > 0; }
                    catch { /* ignore schema detection errors */ }
                }

                // Run migrations
                db.Database.Migrate();
            }
            catch
            {
                throw; // bubble up so host app can handle
            }
        }
    }
}
