namespace Oilgas.Measurements;

/// <summary>
/// Метрика → її канонічна одиниця та символ одиниці. Рядок самої метрики бери через
/// <see cref="EnumWire.Wire"/>; тут лишається лише те, що НЕ виводиться з назви enum'а.
/// </summary>
public static class MeasurementCatalog
{
    /// <summary>Канонічна одиниця метрики (тиск завжди в барах тощо).</summary>
    public static Unit UnitOf(this Measurement m) => m switch
    {
        Measurement.IntakePressure
            or Measurement.TubingPressure
            or Measurement.CasingPressure
            or Measurement.Pressure
            or Measurement.DischargePressure => Unit.Bar,
        Measurement.MotorTemp or Measurement.Temperature => Unit.Celsius,
        Measurement.Vibration => Unit.MillimetrePerSecond,
        Measurement.Rpm => Unit.Rpm,
        Measurement.Level => Unit.Percent,
        Measurement.GasFlow => Unit.CubicMetrePerHour,
        _ => throw new ArgumentOutOfRangeException(nameof(m), m, "Unknown measurement"),
    };

    /// <summary>Символ одиниці для дроту/БД (bar, C, %, mm/s, rpm, m3/h) — не виводиться з назви.</summary>
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
