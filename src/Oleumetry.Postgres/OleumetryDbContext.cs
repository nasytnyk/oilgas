using Microsoft.EntityFrameworkCore;
using Oleumetry.Model;

namespace Oleumetry.Postgres;

/// <summary>
/// EF Core контекст. Мапить POCO-сутності Oleumetry.Model на таблиці Postgres.
/// Конфігурація підключення передається ззовні (DI у хості або design-time factory).
/// </summary>
public class OleumetryDbContext(DbContextOptions<OleumetryDbContext> options) : DbContext(options)
{
    public DbSet<Mine> Mines => Set<Mine>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Tick> Ticks => Set<Tick>();
    public DbSet<Anomaly> Anomalies => Set<Anomaly>();
    public DbSet<Boundary> Boundaries => Set<Boundary>();

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

        b.Entity<Boundary>(e =>
        {
            e.Property(x => x.DeviceType).HasConversion<string>().HasMaxLength(32);
            // один набір меж на (тип обладнання, метрика)
            e.HasIndex(x => new { x.DeviceType, x.Metric }).IsUnique();
        });
    }
}
