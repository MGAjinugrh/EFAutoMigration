using Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace Example.PostgreSql.Data
{
    public class MyDbContext(DbContextOptions<MyDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } = null!;
    }
}
