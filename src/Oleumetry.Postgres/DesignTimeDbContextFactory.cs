using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Oleumetry.Postgres;

/// <summary>
/// Потрібна лише інструменту `dotnet ef` (design-time): дає йому спосіб створити контекст
/// без запущеного хоста. У рантаймі контекст конфігурується через DI у хості.
/// Рядок підключення — з env ConnectionStrings__Postgres, інакше локальний docker-дефолт.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OleumetryDbContext>
{
    public OleumetryDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=oleumetry;Username=oleumetry;Password=oleumetry";

        var options = new DbContextOptionsBuilder<OleumetryDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new OleumetryDbContext(options);
    }
}
