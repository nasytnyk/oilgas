using Microsoft.EntityFrameworkCore;
using Oilgas.Model;

namespace Oilgas.SqlServer;

/// <summary>
/// EF Core контекст. Мапить POCO-сутності Oilgas.Model на таблиці Postgres.
/// Конфігурація підключення передається ззовні (DI у хості або design-time factory).
/// </summary>
public class OilgasDbContext(DbContextOptions<OilgasDbContext> options) : DbContext(options)
{
    public DbSet<Mine> Mines => Set<Mine>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Tick> Ticks => Set<Tick>();
    public DbSet<Anomaly> Anomalies => Set<Anomaly>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Mine (int identity за конвенцією) + зв'язок з Device за конвенцією (Device.MineId → Mine)
        b.Entity<Mine>();

        b.Entity<Device>(e =>
        {
            // enum зберігаємо рядком — читабельно в БД і стабільно при додаванні значень
            e.Property(d => d.Type).HasConversion<string>().HasMaxLength(32);
        });

        b.Entity<Tick>(e =>
        {
            // головний індекс під запити історії/графіків
            e.HasIndex(t => new { t.DeviceId, t.Timestamp });
        });

        b.Entity<Anomaly>(e =>
        {
            e.Property(a => a.Severity).HasConversion<string>().HasMaxLength(16);
            e.HasIndex(a => new { a.DeviceId, a.Timestamp });
        });
    }
}
