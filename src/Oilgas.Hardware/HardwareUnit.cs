using Oilgas.Contracts;
using Oilgas.Measurements;

namespace Oilgas.Hardware;

/// <summary>Одна одиниця обладнання (емулятор): генерує TelemetryMessage і знає свої MQTT-топіки.</summary>
public sealed class HardwareUnit(
    DeviceType type,
    int number,
    Field field,
    Well well,
    IReadOnlyList<MeasurementProfile> profiles)
{
    public DeviceType Type { get; } = type;
    public Field Field { get; } = field;
    public Well Well { get; } = well;

    /// <summary>Id виводиться з типу пристрою + номера: esp_pump-001.</summary>
    public string Id { get; } = $"{type.Wire()}-{number:D3}";

    public string TelemetryTopic => $"oilgas/{Field.Wire()}/{Well.Wire()}/{Type.Wire()}/{Id}/telemetry";
    public string StatusTopic    => $"oilgas/{Field.Wire()}/{Well.Wire()}/{Type.Wire()}/{Id}/status";

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
        return new TelemetryMessage(Id, Type.Wire(), Field.Wire(), Well.Wire(), now, samples);
    }

    public DeviceStatusMessage BuildStatus(DeviceState state, DateTimeOffset now) =>
        new(Id, state.Wire(), now);
}
