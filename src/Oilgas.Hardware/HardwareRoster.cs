using Oilgas.Measurements;

namespace Oilgas.Hardware;

/// <summary>Фіксований набір одиниць обладнання різних типів (демо-«поле»).</summary>
public static class HardwareRoster
{
    public static IReadOnlyList<HardwareUnit> Build() =>
    [
        new("esp-001", DeviceType.EspPump, "north", "w-12",
        [
            //                       metric                        base  noise  spike%  ±
            new(Measurement.IntakePressure,  80,   8, 0.05, 0.5),
            new(Measurement.MotorTemp,       95,   6, 0.05, 0.3),
            new(Measurement.Vibration,        3,   1, 0.05, 0.6),
            new(Measurement.Rpm,           3500,  80, 0,    0),
        ]),
        new("wh-001", DeviceType.Wellhead, "north", "w-12",
        [
            new(Measurement.TubingPressure, 150, 10, 0.05, 0.4),
            new(Measurement.CasingPressure,  90,  6, 0.05, 0.4),
            new(Measurement.Temperature,     70,  4, 0.03, 0.35),
        ]),
        new("sep-001", DeviceType.Separator, "north", "cpf",
        [
            new(Measurement.Pressure, 12, 1.5, 0.04, 0.5),
            new(Measurement.Level,    60,   8, 0.04, 0.4),
            new(Measurement.GasFlow, 500,  40, 0,    0),
        ]),
        new("cmp-001", DeviceType.Compressor, "north", "cpf",
        [
            new(Measurement.DischargePressure, 40, 4, 0.05, 0.5),
            new(Measurement.Temperature,       85, 6, 0.05, 0.35),
            new(Measurement.Vibration,          4, 1, 0.05, 0.6),
        ]),
    ];
}
