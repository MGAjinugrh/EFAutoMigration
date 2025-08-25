using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Example.PostgreSql.Data.Factories
{
    public class MyDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
    {
        public MyDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=testdb;Username=postgres;Password=password");

            return new MyDbContext(optionsBuilder.Options);
        }
    }
}
