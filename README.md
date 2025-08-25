# EFAutoMigration

`EFAutoMigration` is a small .NET helper library designed to **automatically apply Entity Framework Core migrations at runtime**.  
It removes the need to run `dotnet ef database update` manually when deploying your application.  
Instead, your database schema is automatically migrated when the application starts.

This library works with **.NET Standard 2.0** and can be consumed by higher frameworks such as **.NET 8.0**.

---

## Features
- Auto-applies pending EF Core migrations on application startup.  
- Supports PostgreSQL (Npgsql provider).  
- Supports MySQL / MariaDB (Pomelo provider).  
- Works with both Console apps and ASP.NET Core apps.  
- Compatible with .NET Standard 2.0 and .NET 8.0 projects.  

---

## Dependencies

Your project must include the EF Core provider for the database you use:

### Common
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Relational`
- `Microsoft.EntityFrameworkCore.Design`

### PostgreSQL
- `Npgsql.EntityFrameworkCore.PostgreSQL`

### MySQL (Pomelo recommended)
- `Pomelo.EntityFrameworkCore.MySql`

### Optional (for hosting integration)
- `Microsoft.Extensions.Hosting`

---

## Installation

Clone or reference this project as a library in your solution:

```xml
<ProjectReference Include="..\EfAutoMigration\EfAutoMigration.csproj" />