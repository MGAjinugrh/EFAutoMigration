using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EfAutoMigration.Interfaces;

public interface ISeeder<TContext> where TContext : DbContext
{
    Task SeedAsync(TContext context);
}
