using Microsoft.EntityFrameworkCore;
using Oilgas.Model;

namespace Oilgas.SqlServer;

/// <summary>
/// EF Core контекст. Мапить POCO-сутність Tick на таблицю Azure SQL.
/// Конфігурація підключення передається ззовні (DI у хості або design-time factory).
/// </summary>
public class OilgasDbContext(DbContextOptions<OilgasDbContext> options) : DbContext(options)
{
    public DbSet<Tick> Ticks => Set<Tick>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Tick>(e =>
        {
            // головний індекс під запити історії/графіків
            e.HasIndex(t => new { t.DeviceId, t.Timestamp });
        });
    }
}
