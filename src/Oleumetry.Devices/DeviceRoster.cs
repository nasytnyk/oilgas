namespace Oleumetry.Devices;

/// <summary>Фіксований набір емульованих пристроїв різних типів (демо-«поле»).</summary>
public static class DeviceRoster
{
    public static IReadOnlyList<Device> Build() =>
    [
        new("esp-001", "EspPump", "north", "w-12",
        [
            new MetricSpec("intake_pressure", "bar",  80, 8, 0.05, 40),
            new MetricSpec("motor_temp",      "C",    95, 6, 0.05, 30),
            new MetricSpec("vibration",       "mm/s",  3, 1, 0.05,  6),
            new MetricSpec("rpm",             "rpm", 3500, 80, 0,   0),
        ]),
        new("wh-001", "Wellhead", "north", "w-12",
        [
            new MetricSpec("tubing_pressure", "bar", 150, 10, 0.05, 60),
            new MetricSpec("casing_pressure", "bar",  90,  6, 0.05, 40),
            new MetricSpec("temperature",     "C",    70,  4, 0.03, 25),
        ]),
        new("sep-001", "Separator", "north", "cpf",
        [
            new MetricSpec("pressure", "bar",  12, 1.5, 0.04,  8),
            new MetricSpec("level",    "%",    60,   8, 0.04, 30),
            new MetricSpec("gas_flow", "m3/h", 500, 40, 0,     0),
        ]),
        new("cmp-001", "Compressor", "north", "cpf",
        [
            new MetricSpec("discharge_pressure", "bar", 40, 4, 0.05, 20),
            new MetricSpec("temperature",        "C",   85, 6, 0.05, 30),
            new MetricSpec("vibration",          "mm/s", 4, 1, 0.05,  6),
        ]),
    ];
}
