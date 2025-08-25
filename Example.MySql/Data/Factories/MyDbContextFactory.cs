using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Example.MySql.Data.Factories
{

    public class MyDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
    {
        public MyDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
            var connectionString = "Server=localhost;Database=testdb;User=root;Password=password;";

            optionsBuilder.UseMySql(connectionString,
                ServerVersion.AutoDetect(connectionString));

            return new MyDbContext(optionsBuilder.Options);
        }
    }
}
