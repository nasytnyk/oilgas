namespace Oleumetry.Domain.Telemetry;

/// <summary>
/// Числовий діапазон [Min; Max]. Багаторазовий VO (використовується у MetricDefinition і Threshold).
/// Назва NumericRange, а не Range — щоб не плутати з System.Range.
/// Для односторонніх меж використовуй double.NegativeInfinity / PositiveInfinity.
/// </summary>
public sealed record NumericRange
{
    public double Min { get; }
    public double Max { get; }

    public NumericRange(double min, double max)
    {
        if (min > max)
            throw new ArgumentException($"Min ({min}) не може бути більшим за Max ({max}).");
        Min = min;
        Max = max;
    }

    public bool Contains(double value) => value >= Min && value <= Max;
}
