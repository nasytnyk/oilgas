using Microsoft.EntityFrameworkCore;
using Oilgas.Model;
using Oilgas.Ef;

namespace Oilgas.Ui;

/// <summary>GraphQL queries — читання історії/станів з Azure SQL.</summary>
public sealed class Query
{
    /// <summary>Пристрої та їх стан (виведено з останніх тіків).</summary>
    public async Task<IReadOnlyList<DeviceState>> GetDevices(
        IDbContextFactory<OilgasDbContext> dbf, CancellationToken ct)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);
        var rows = await db.Ticks
            .GroupBy(t => t.DeviceId)
            .Select(g => new { Id = g.Key, Last = g.Max(x => x.Timestamp) })
            .ToListAsync(ct);
        return rows
            .Select(r => new DeviceState(r.Id, r.Last))
            .OrderBy(d => d.DeviceId)
            .ToList();
    }

    /// <summary>Останні N тіків пристрою (опційно — конкретна метрика).</summary>
    public async Task<IReadOnlyList<Tick>> GetTicks(
        IDbContextFactory<OilgasDbContext> dbf, string deviceId, string? metric, int last, CancellationToken ct)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);
        var q = db.Ticks.Where(t => t.DeviceId == deviceId);
        if (!string.IsNullOrWhiteSpace(metric)) q = q.Where(t => t.Metric == metric);
        return await q.OrderByDescending(t => t.Timestamp)
            .Take(Math.Clamp(last, 1, 500))
            .ToListAsync(ct);
    }

    /// <summary>Лічильники для діагностики.</summary>
    public async Task<Stats> GetStats(IDbContextFactory<OilgasDbContext> dbf, CancellationToken ct)
    {
        await using var db = await dbf.CreateDbContextAsync(ct);
        return new(await db.Ticks.LongCountAsync(ct),
                   await db.Ticks.Select(t => t.DeviceId).Distinct().CountAsync(ct));
    }
}
