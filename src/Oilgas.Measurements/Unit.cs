namespace Oilgas.Measurements;

/// <summary>
/// Одиниця виміру. Закритий набір — джерело правди для всіх шарів.
/// Символ для дроту/БД бери через <see cref="MeasurementCatalog.Symbol"/>.
/// </summary>
public enum Unit
{
    Bar,
    Celsius,
    Percent,
    MillimetrePerSecond,
    Rpm,
    CubicMetrePerHour,
}
