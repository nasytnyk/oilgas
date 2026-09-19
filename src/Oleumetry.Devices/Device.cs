using Oleumetry.Contracts;

namespace Oleumetry.Devices;

/// <summary>Опис метрики для генерації: базове значення, шум і шанс/величина сплеску (для алармів).</summary>
public sealed record MetricSpec(
    string Name, string Unit, double Baseline, double Noise, double SpikeChance, double SpikeDelta);

/// <summary>
/// Один пристрій-емулятор: генерує TelemetryMessage і знає свої MQTT-топіки.
/// Чистий (без MQTT) — публікацією керує Worker.
/// </summary>
public sealed class Device(
    string id, string type, string field, string well, IReadOnlyList<MetricSpec> metrics)
{
    public string Id { get; } = id;
    public string Type { get; } = type;
    public string Field { get; } = field;
    public string Well { get; } = well;

    public string TelemetryTopic => $"og/{Field}/{Well}/{Type}/{Id}/telemetry";
    public string StatusTopic    => $"og/{Field}/{Well}/{Type}/{Id}/status";

    public TelemetryMessage BuildTelemetry(DateTimeOffset now)
    {
        var samples = new List<TickSample>(metrics.Count);
        foreach (var m in metrics)
        {
            var value = m.Baseline + (Random.Shared.NextDouble() - 0.5) * 2 * m.Noise;
            if (Random.Shared.NextDouble() < m.SpikeChance) value += m.SpikeDelta;
            samples.Add(new TickSample(m.Name, Math.Round(value, 2), m.Unit));
        }
        return new TelemetryMessage(Id, Type, Field, Well, now, samples);
    }

    public DeviceStatusMessage BuildStatus(string status, DateTimeOffset now) => new(Id, status, now);
}
