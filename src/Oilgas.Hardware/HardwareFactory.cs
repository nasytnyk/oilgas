using Oilgas.Measurements;

namespace Oilgas.Hardware;

/// <summary>
/// Будує набір одиниць обладнання з визначень (hardware.json), резолвлячи метрики профілів
/// через <see cref="MeasurementRegistry"/>. Дані — зовні; тут лише збірка.
/// </summary>
public static class HardwareFactory
{
    public static IReadOnlyList<HardwareUnit> Build(
        IReadOnlyList<DeviceDefinition> definitions, MeasurementRegistry measurements)
    {
        if (definitions is null || definitions.Count == 0)
            throw new InvalidOperationException("hardware.json: жодного пристрою не задано.");

        var units = new List<HardwareUnit>(definitions.Count);
        foreach (var d in definitions)
        {
            if (string.IsNullOrWhiteSpace(d.Type) || string.IsNullOrWhiteSpace(d.Field) || string.IsNullOrWhiteSpace(d.Well))
                throw new InvalidOperationException($"hardware.json: пристрій із неповною топологією (type/field/well).");

            var profiles = d.Profiles
                .Select(p => new MeasurementProfile(
                    measurements.FromWire(p.Measurement),
                    p.Baseline, p.Noise, p.SpikeChance, p.SpikePercent))
                .ToList();

            units.Add(new HardwareUnit(d.Type, d.Number, d.Field, d.Well, profiles));
        }
        return units;
    }
}
