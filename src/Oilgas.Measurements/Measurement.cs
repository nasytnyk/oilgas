using System.Reflection;

namespace Oilgas.Measurements;

/// <summary>
/// Що вимірюємо — «smart enum»: закритий набір статичних екземплярів, кожен несе ІНТРИНСИВНЕ
/// метриці (одиниця, фізичні межі, точність, алармові пороги). Джерело правди для всіх шарів.
/// Те, що залежить від конкретного пристрою (baseline/noise/spike), тут НЕ живе — це тюнінг
/// профілю пристрою (MeasurementProfile).
/// На дріт/у БД мандрує як <see cref="Wire"/> (snake_case); назад — через <see cref="FromWire"/>.
/// </summary>
public sealed class Measurement
{
    public static readonly Measurement IntakePressure =
        new(nameof(IntakePressure), Unit.Bar, floor: 0, ceiling: 500, decimals: 1,
            warning: new(Max: 120), critical: new(Max: 150));

    public static readonly Measurement MotorTemp =
        new(nameof(MotorTemp), Unit.Celsius, floor: 0, ceiling: 250, decimals: 1,
            warning: new(Max: 110), critical: new(Max: 130));

    public static readonly Measurement Vibration =
        new(nameof(Vibration), Unit.MillimetrePerSecond, floor: 0, ceiling: 50, decimals: 2,
            warning: new(Max: 7), critical: new(Max: 11));

    public static readonly Measurement Rpm =
        new(nameof(Rpm), Unit.Rpm, floor: 0, ceiling: 6000, decimals: 0);

    public static readonly Measurement TubingPressure =
        new(nameof(TubingPressure), Unit.Bar, floor: 0, ceiling: 700, decimals: 1,
            warning: new(Max: 220), critical: new(Max: 260));

    public static readonly Measurement CasingPressure =
        new(nameof(CasingPressure), Unit.Bar, floor: 0, ceiling: 500, decimals: 1,
            warning: new(Max: 140), critical: new(Max: 170));

    public static readonly Measurement Temperature =
        new(nameof(Temperature), Unit.Celsius, floor: -50, ceiling: 300, decimals: 1,
            warning: new(Max: 120), critical: new(Max: 150));

    public static readonly Measurement Pressure =
        new(nameof(Pressure), Unit.Bar, floor: 0, ceiling: 100, decimals: 1,
            warning: new(Max: 20), critical: new(Max: 25));

    public static readonly Measurement Level =
        new(nameof(Level), Unit.Percent, floor: 0, ceiling: 100, decimals: 1,
            warning: new(Min: 20, Max: 90), critical: new(Min: 10, Max: 95));

    public static readonly Measurement GasFlow =
        new(nameof(GasFlow), Unit.CubicMetrePerHour, floor: 0, ceiling: 5000, decimals: 0);

    public static readonly Measurement DischargePressure =
        new(nameof(DischargePressure), Unit.Bar, floor: 0, ceiling: 300, decimals: 1,
            warning: new(Max: 60), critical: new(Max: 75));

    private Measurement(
        string name, Unit unit, double floor, double ceiling, int decimals,
        Threshold? warning = null, Threshold? critical = null)
    {
        Name = name;
        Wire = EnumWire.ToSnakeCase(name);
        Unit = unit;
        Floor = floor;
        Ceiling = ceiling;
        Decimals = decimals;
        Warning = warning;
        Critical = critical;
    }

    public string Name { get; }        // PascalCase, як у коді
    public string Wire { get; }        // snake_case, на дріт/у БД
    public Unit Unit { get; }          // канонічна одиниця
    public double Floor { get; }       // фізична нижня межа (клампимо симуляцію)
    public double Ceiling { get; }     // фізична верхня межа
    public int Decimals { get; }       // точність округлення на дроті
    public Threshold? Warning { get; }
    public Threshold? Critical { get; }

    // Реєстр усіх метрик — зібраний рефлексією зі статичних полів, щоб нічого не забути.
    private static readonly IReadOnlyDictionary<string, Measurement> ByWire =
        typeof(Measurement)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(Measurement))
            .Select(f => (Measurement)f.GetValue(null)!)
            .ToDictionary(m => m.Wire);

    public static IReadOnlyCollection<Measurement> All => (IReadOnlyCollection<Measurement>)ByWire.Values;

    public static Measurement FromWire(string wire) =>
        ByWire.TryGetValue(wire, out var m)
            ? m
            : throw new ArgumentOutOfRangeException(nameof(wire), wire, "Unknown measurement");

    public override string ToString() => Wire;
}
