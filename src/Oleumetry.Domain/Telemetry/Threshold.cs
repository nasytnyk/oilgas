using Oleumetry.Domain.Alarms;

namespace Oleumetry.Domain.Telemetry;

/// <summary>
/// Пороги для однієї метрики: смуга Warning і (ширша) смуга Critical.
/// Серце майбутнього Device.Evaluate — тут вирішується, чи значення породжує аларм і якої серйозності.
/// </summary>
public sealed record Threshold
{
    public string MetricName { get; }
    public NumericRange Warning { get; }
    public NumericRange Critical { get; }

    public Threshold(string metricName, NumericRange warning, NumericRange critical)
    {
        if (string.IsNullOrWhiteSpace(metricName))
            throw new ArgumentException("MetricName не може бути порожнім.", nameof(metricName));
        // Critical мусить охоплювати Warning: спершу застереження, потім критично.
        if (critical.Min > warning.Min || critical.Max < warning.Max)
            throw new ArgumentException("Critical-смуга має охоплювати Warning-смугу.");

        MetricName = metricName;
        Warning = warning;
        Critical = critical;
    }

    /// <summary>Серйозність порушення, або null — якщо значення в нормі.</summary>
    public Severity? Evaluate(double value)
    {
        if (!Critical.Contains(value)) return Severity.Critical;
        if (!Warning.Contains(value)) return Severity.Warning;
        return null;
    }
}
