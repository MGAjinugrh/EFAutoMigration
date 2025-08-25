using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace EfAutoMigration
{
    public static class AutoMigrationExtensions
    {
        /// <summary>
        /// Registers an <see cref="IStartupFilter"/> that will run
        /// <c>Database.Migrate()</c> during application startup.
        /// Works with PostgreSQL, MySQL, and other EFCore-supported providers.
        /// </summary>
        /// <typeparam name="TContext">Your <see cref="DbContext"/> type.</typeparam>
        /// <param name="services">The application's DI container.</param>
        /// <param name="markerTables">
        /// One or more table names that must exist to consider the schema "present".
        /// If none exist, migrations run; otherwise only pending migrations run.
        /// </param>
        public static IServiceCollection AddEfAutoMigration<TContext>(
            this IServiceCollection services,
            params string[] markerTables)
            where TContext : DbContext
        {
            services.AddSingleton<IStartupFilter>(
                new AutoMigrationStartupFilter<TContext>(markerTables));
            return services;
        }

        /* ---------- nested private class ---------- */

        private sealed class AutoMigrationStartupFilter<TContext> : IStartupFilter
            where TContext : DbContext
        {
            private readonly string[] _markers;
            public AutoMigrationStartupFilter(string[] markers)
            {
                _markers = markers ?? new string[0];
            }

            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                return app =>
                {
                    using (var scope = app.ApplicationServices.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<TContext>();

                        try
                        {
                            // Make sure database exists first
                            db.Database.EnsureCreated();

                            bool schemaExists = false;
                            var provider = db.Database.ProviderName ?? string.Empty;

                            if (_markers.Length > 0 && db.Database.CanConnect())
                            {
                                var markerList = string.Join(",", _markers.Select(t => "'" + t + "'"));

                                string sql;

                                if (provider.Contains("Npgsql"))
                                {
                                    sql = "SELECT 1 FROM pg_tables WHERE tablename IN (" + markerList + ") LIMIT 1";
                                }
                                else if (provider.Contains("MySql") || provider.Contains("MariaDb"))
                                {
                                    sql = "SELECT 1 FROM information_schema.tables WHERE table_name IN (" + markerList + ") LIMIT 1";
                                }
                                else
                                {
                                    // Generic fallback
                                    sql = "SELECT 1 FROM information_schema.tables WHERE table_name IN (" + markerList + ") LIMIT 1";
                                }

                                try
                                {
                                    schemaExists = db.Database.ExecuteSqlRaw(sql) > 0;
                                }
                                catch
                                {
                                    // ignore schema detection failure, just run migrations
                                }
                            }

                            db.Database.Migrate();
                        }
                        catch
                        {
                            throw; // bubble up so host app can decide how to handle errors
                        }
                    }

                    next(app);
                };
            }
        }
    }
}
