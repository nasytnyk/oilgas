using Oilgas.Contracts;
using Oilgas.Measurements;

namespace Oilgas.Hardware;

/// <summary>Одна одиниця обладнання (емулятор): генерує TelemetryMessage і знає свої MQTT-топіки.</summary>
public sealed class HardwareUnit(
    string id,
    DeviceType type,
    string field,
    string well,
    IReadOnlyList<MeasurementProfile> profiles)
{
    public string Id { get; } = id;
    public DeviceType Type { get; } = type;
    public string Field { get; } = field;
    public string Well { get; } = well;

    public string TelemetryTopic => $"oilgas/{Field}/{Well}/{Type.Wire()}/{Id}/telemetry";
    public string StatusTopic    => $"oilgas/{Field}/{Well}/{Type.Wire()}/{Id}/status";

    public TelemetryMessage BuildTelemetry(DateTimeOffset now)
    {
        var samples = new List<TickSample>(profiles.Count);
        foreach (var p in profiles)
        {
            var value = p.Baseline + (Random.Shared.NextDouble() - 0.5) * 2 * p.Noise;
            if (Random.Shared.NextDouble() < p.SpikeChance)
            {
                var sign = Random.Shared.Next(2) == 0 ? -1 : 1; // сплеск у довільну сторону
                value *= 1 + sign * p.SpikePercent;
            }
            samples.Add(new TickSample(
                p.Measurement.Wire(),
                Math.Round(value, 2),
                p.Measurement.UnitOf().Symbol()));
        }
        return new TelemetryMessage(Id, Type.Wire(), Field, Well, now, samples);
    }

    public DeviceStatusMessage BuildStatus(DeviceState state, DateTimeOffset now) =>
        new(Id, state.Wire(), now);
}
