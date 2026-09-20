namespace Oilgas.Measurements;

/// <summary>
/// Що вимірюємо — доменний об'єкт з інтринсивними властивостями метрики (одиниця, фізичні межі,
/// точність, алармові пороги). Екземпляри створює <see cref="MeasurementRegistry"/> з конфіга
/// (measurements.json) — джерело правди винесене в дані. На дріт/у БД мандрує як <see cref="Wire"/>.
/// Per-device тюнінг (baseline/noise/spike) тут НЕ живе — це профіль пристрою.
/// </summary>
public sealed class Measurement(
    string name, string wire, Unit unit, double floor, double ceiling, int decimals,
    Threshold? warning, Threshold? critical)
{
    public string Name { get; } = name;      // PascalCase, як у measurements.json
    public string Wire { get; } = wire;       // snake_case, на дріт/у БД
    public Unit Unit { get; } = unit;
    public double Floor { get; } = floor;     // фізична нижня межа (клампимо симуляцію)
    public double Ceiling { get; } = ceiling; // фізична верхня межа
    public int Decimals { get; } = decimals;  // точність округлення на дроті
    public Threshold? Warning { get; } = warning;
    public Threshold? Critical { get; } = critical;

    public override string ToString() => Wire;
}
