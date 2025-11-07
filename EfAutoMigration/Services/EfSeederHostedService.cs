using EfAutoMigration.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EfAutoMigration.Services;

internal class EfSeederHostedService<TContext> : IHostedService
        where TContext : DbContext
{
    private readonly IServiceProvider _services;
    private readonly ILogger<EfSeederHostedService<TContext>> _logger;

    public EfSeederHostedService(
        IServiceProvider services,
        ILogger<EfSeederHostedService<TContext>> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        var seeders = scope.ServiceProvider.GetServices<ISeeder<TContext>>();

        foreach (var seeder in seeders)
        {
            try
            {
                await seeder.SeedAsync(context);
                _logger.LogInformation("✅ Seeder {Seeder} executed successfully.", seeder.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error executing seeder {Seeder}", seeder.GetType().Name);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
