namespace Oleumetry.Domain.Telemetry;

/// <summary>
/// Довідникове визначення метрики: назва, одиниця та фізично правдоподібний діапазон.
/// Частина DeviceTypeProfile; вантажиться з БД.
/// </summary>
public sealed record MetricDefinition(string Name, string Unit, NumericRange ValidRange)
{
    /// <summary>Чи значення взагалі фізично правдоподібне (відсіює биті дані/шум сенсора).</summary>
    public bool IsPlausible(double value) => ValidRange.Contains(value);
}
