using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Oilgas.Postgres;

/// <summary>
/// Потрібна лише інструменту `dotnet ef` (design-time): дає йому спосіб створити контекст
/// без запущеного хоста. У рантаймі контекст конфігурується через DI у хості.
/// Рядок підключення — з env ConnectionStrings__Postgres, інакше локальний docker-дефолт.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OilgasDbContext>
{
    public OilgasDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=oilgas;Username=oilgas;Password=oilgas";

        var options = new DbContextOptionsBuilder<OilgasDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new OilgasDbContext(options);
    }
}
