namespace Oilgas.Measurements;

/// <summary>Символ одиниці для дроту/БД — єдина мапа, яку НЕ вивести з назви (Celsius → "C").</summary>
public static class UnitExtensions
{
    public static string Symbol(this Unit u) => u switch
    {
        Unit.Bar => "bar",
        Unit.Celsius => "C",
        Unit.Percent => "%",
        Unit.MillimetrePerSecond => "mm/s",
        Unit.Rpm => "rpm",
        Unit.CubicMetrePerHour => "m3/h",
        _ => throw new ArgumentOutOfRangeException(nameof(u), u, "Unknown unit"),
    };
}
