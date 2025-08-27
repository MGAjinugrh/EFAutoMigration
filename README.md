# EFAutoMigration

`EFAutoMigration` is a lightweight .NET helper library that ensures **Entity Framework Core migrations are automatically applied at runtime**.\
It removes the need to manually call `Database.Migrate()` in every project or run `dotnet ef database update` during deployment.\
Instead, migrations are handled consistently when the application starts.

The library targets **.NET Standard 2.0** (maximum compatibility), and can be consumed seamlessly in modern frameworks such as **.NET 8.0**.

---

## ✨ Features

- Auto-applies pending EF Core migrations at startup.
- Supports **PostgreSQL (Npgsql), MySQL/MariaDB (Pomelo)** and **SQL Server (EFCore)**.
- Works with any application using the **.NET Generic Host** model (ASP.NET Core, console apps, worker services, etc.).
- Marker table support → detect if schema exists before applying migrations.
- Removes boilerplate `Migrate()` code across multiple DbContexts.
- Built on .NET Standard 2.0 → works in .NET Framework 4.6.1+ and .NET Core/5/6/7/8+ projects.

---

## 🚦 When to Use

✅ **Best suited for**: Production apps, ASP.NET Core microservices, or systems with multiple DbContexts and evolving migrations.\
⚠️ **Not necessary for**: Small console utilities or prototypes where `EnsureCreated()` is enough.

---

## 🔑 EnsureCreated() vs. Migrate()

Entity Framework Core provides two different ways to get your database schema in place:

### `EnsureCreated()`
- Creates the database and schema directly from your current DbContext model.
- Bypasses the migration system entirely.
- Good for **prototypes, tests, or throwaway apps**.
- ❌ Cannot evolve schema later. If you add/remove columns, you must drop and recreate the whole database.

### `Migrate()`
- Requires migrations to be created via `dotnet ef migrations add`.
- Applies migrations incrementally and tracks history in the `__EFMigrationsHistory` table.
- ✅ Production-ready. Safely evolves schema as your app grows.

👉 `EFAutoMigration` is built around **`Migrate()`**, not `EnsureCreated()`. Use it when you want your application to automatically apply pending migrations on startup.

---

## 📦 Dependencies

### Common

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Relational`
- `Microsoft.EntityFrameworkCore.Design`

### PostgreSQL

- `Npgsql.EntityFrameworkCore.PostgreSQL`

### MySQL (Pomelo recommended)

- `Pomelo.EntityFrameworkCore.MySql`

### SQL Server

- `Microsoft.EntityFrameworkCore.SqlServer`

### Optional (for hosting integration)

- `Microsoft.Extensions.Hosting`

---

## 🚀 Installation

Reference the NuGet package:

```sh
dotnet add package EFAutoMigration
```

Or reference locally in your solution:

```xml
<ProjectReference Include="..\EfAutoMigration\EfAutoMigration.csproj" />
```

---

## 🛠 Usage

### Conventional vs. EFAutoMigration

| Scenario | Conventional EF Core | With EFAutoMigration |
|----------|----------------------|----------------------|
| Apply migrations | `context.Database.Migrate();` everywhere manually | Auto-applied once at startup |
| Multiple DbContexts | Must repeat `Migrate()` for each | Centralized handling for all DbContexts |
| Boilerplate | Clutters Program.cs / Startup.cs | Clean DI registration only |
| Schema detection | Must implement manually | Built-in marker table check |
| Scaling to microservices | Error-prone, easy to forget | Consistent and automatic |

---

### Without EFAutoMigration (conventional)

```csharp
using var context = new MyDbContext(...);
context.Database.Migrate();
```

This works, but you must repeat it everywhere, and it doesn’t scale well across multiple microservices or DbContexts.

---

### With EFAutoMigration (recommended)

**Program.cs (ASP.NET Core / microservice style)**

```csharp
services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=testdb;Username=postgres;Password=yourpassword")); // depend on your db engine

services.AddEfAutoMigration<MyDbContext>("user");
```

Now migrations are:

- Automatically applied at startup.
- Aware of whether schema already exists (via marker tables).
- Centralized → no boilerplate across services.

---

### Example: Console App (.NET 8 + PostgreSQL)

```csharp
using Example.PostgreSql.Data;
using EfAutoMigration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<MyDbContext>(options =>
                    options.UseNpgsql("Host=localhost;Database=testdb;Username=postgres;Password=yourpassword"));

                services.AddEfAutoMigration<MyDbContext>("user");
            })
            .Build();

        await host.StartAsync();
        Console.WriteLine("✅ Database migrated via EfAutoMigration");

        await host.StopAsync();
    }
}
```

---



## 📝 Notes

* On **first run with a brand new database**, you may see a log entry like:

```fail: Microsoft.EntityFrameworkCore.Database.Command[20102]
      Failed executing DbCommand ...
      SELECT "MigrationId", "ProductVersion"
      FROM "__EFMigrationsHistory"
```
This is **expected**. EF Core is checking for its migration history table, which doesn’t exist until the first migration is applied.
The library will still create the database and apply migrations successfully.

---

## 📖 License

This project is licensed under the [MIT License](LICENSE).

