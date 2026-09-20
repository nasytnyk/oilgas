namespace Oilgas.Measurements;

/// <summary>
/// Джерело правди для метрик: канонічна одиниця кожної метрики та рядкові представлення
/// для дроту (MQTT/JSON) і БД. Enum усередині — людський рядок зовні.
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

    /// <summary>Стабільний рядок метрики для дроту/БД (snake_case).</summary>
    public static string Wire(this Measurement m) => m switch
    {
        Measurement.IntakePressure => "intake_pressure",
        Measurement.MotorTemp => "motor_temp",
        Measurement.Vibration => "vibration",
        Measurement.Rpm => "rpm",
        Measurement.TubingPressure => "tubing_pressure",
        Measurement.CasingPressure => "casing_pressure",
        Measurement.Temperature => "temperature",
        Measurement.Pressure => "pressure",
        Measurement.Level => "level",
        Measurement.GasFlow => "gas_flow",
        Measurement.DischargePressure => "discharge_pressure",
        _ => throw new ArgumentOutOfRangeException(nameof(m), m, "Unknown measurement"),
    };

    /// <summary>Символ одиниці для дроту/БД (bar, C, %, mm/s, rpm, m3/h).</summary>
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
