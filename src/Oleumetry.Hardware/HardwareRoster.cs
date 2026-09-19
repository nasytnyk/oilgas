namespace Oleumetry.Hardware;

/// <summary>Фіксований набір одиниць обладнання різних типів (демо-«поле»).</summary>
public static class HardwareRoster
{
    public static IReadOnlyList<HardwareUnit> Build() =>
    [
        new("esp-001", "EspPump", "north", "w-12",
        [
            new MetricProfile("intake_pressure", "bar",  80,  8, 0.05, 40),
            new MetricProfile("motor_temp",      "C",    95,  6, 0.05, 30),
            new MetricProfile("vibration",       "mm/s",  3,  1, 0.05,  6),
            new MetricProfile("rpm",             "rpm", 3500, 80, 0,    0),
        ]),
        new("wh-001", "Wellhead", "north", "w-12",
        [
            new MetricProfile("tubing_pressure", "bar", 150, 10, 0.05, 60),
            new MetricProfile("casing_pressure", "bar",  90,  6, 0.05, 40),
            new MetricProfile("temperature",     "C",    70,  4, 0.03, 25),
        ]),
        new("sep-001", "Separator", "north", "cpf",
        [
            new MetricProfile("pressure", "bar",  12, 1.5, 0.04,  8),
            new MetricProfile("level",    "%",    60,   8, 0.04, 30),
            new MetricProfile("gas_flow", "m3/h", 500,  40, 0,     0),
        ]),
        new("cmp-001", "Compressor", "north", "cpf",
        [
            new MetricProfile("discharge_pressure", "bar", 40, 4, 0.05, 20),
            new MetricProfile("temperature",        "C",   85, 6, 0.05, 30),
            new MetricProfile("vibration",          "mm/s", 4, 1, 0.05,  6),
        ]),
    ];
}
