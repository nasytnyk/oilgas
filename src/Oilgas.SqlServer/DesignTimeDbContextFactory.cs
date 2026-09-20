using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Oilgas.SqlServer;

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
            ?? "Server=localhost,1433;Database=oilgas;User Id=sa;Password=Oilgas!Local1;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<OilgasDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new OilgasDbContext(options);
    }
}
