namespace Oilgas.Measurements;

/// <summary>
/// Реєстр метрик, зібраний з визначень (measurements.json). Джерело правди в рантаймі:
/// пошук за wire, перелік усіх. Реєструється як singleton і інжектиться туди, де потрібен.
/// </summary>
public sealed class MeasurementRegistry
{
    private readonly IReadOnlyDictionary<string, Measurement> _byWire;

    public MeasurementRegistry(IReadOnlyList<MeasurementDefinition> definitions)
    {
        if (definitions is null || definitions.Count == 0)
            throw new InvalidOperationException("measurements.json: жодної метрики не задано.");

        var map = new Dictionary<string, Measurement>(definitions.Count);
        foreach (var d in definitions)
        {
            if (string.IsNullOrWhiteSpace(d.Name))
                throw new InvalidOperationException("measurements.json: метрика без Name.");

            var wire = EnumWire.ToSnakeCase(d.Name);
            var m = new Measurement(d.Name, wire, d.Unit, d.Floor, d.Ceiling, d.Decimals,
                ToThreshold(d.Warning), ToThreshold(d.Critical));

            if (!map.TryAdd(wire, m))
                throw new InvalidOperationException($"measurements.json: дублікат метрики '{wire}'.");
        }

        _byWire = map;
    }

    public IReadOnlyCollection<Measurement> All => (IReadOnlyCollection<Measurement>)_byWire.Values;

    public Measurement FromWire(string wire) =>
        _byWire.TryGetValue(wire, out var m)
            ? m
            : throw new KeyNotFoundException($"Невідома метрика '{wire}' (нема в measurements.json).");

    private static Threshold? ToThreshold(ThresholdDefinition? d) =>
        d is null ? null : new Threshold(d.Min, d.Max);
}
