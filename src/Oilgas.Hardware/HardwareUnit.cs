using Oilgas.Model;

namespace Oilgas.Hardware;

/// <summary>
/// Одна одиниця обладнання (емулятор): генерує TelemetryMessage і знає свої MQTT-топіки.
/// Топологія (type/field/well) — рядки з hardware.json (data-driven), тому тут не enum-и.
/// </summary>
public sealed class HardwareUnit(
    string type,
    int number,
    string field,
    string well,
    IReadOnlyList<MeasurementProfile> profiles)
{
    public string Type { get; } = type;   // wire-рядок, напр. "esp_pump"
    public string Field { get; } = field; // напр. "north"
    public string Well { get; } = well;   // напр. "w12"

    /// <summary>Id виводиться з типу + номера: esp_pump-001.</summary>
    public string Id { get; } = $"{type}-{number:D3}";

    public string TelemetryTopic => $"oilgas/{Field}/{Well}/{Type}/{Id}/telemetry";
    public string StatusTopic    => $"oilgas/{Field}/{Well}/{Type}/{Id}/status";

    public TelemetryMessage BuildTelemetry(DateTimeOffset now)
    {
        var samples = new List<TickSample>(profiles.Count);
        foreach (var p in profiles)
        {
            var m = p.Measurement;
            var value = p.Baseline + (Random.Shared.NextDouble() - 0.5) * 2 * p.Noise;
            if (Random.Shared.NextDouble() < p.SpikeChance)
            {
                var sign = Random.Shared.Next(2) == 0 ? -1 : 1; // сплеск у довільну сторону
                value *= 1 + sign * p.SpikePercent;
            }
            value = Math.Clamp(value, m.Floor, m.Ceiling);       // не виходимо за фізичні межі
            samples.Add(new TickSample(m.Wire, Math.Round(value, m.Decimals), m.Unit.Symbol()));
        }
        return new TelemetryMessage(Id, Type, Field, Well, now, samples);
    }

    public DeviceStatusMessage BuildStatus(DeviceState state, DateTimeOffset now) =>
        new(Id, state.Wire(), now);
}
