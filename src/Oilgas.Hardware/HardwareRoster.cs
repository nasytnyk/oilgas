using Oilgas.Measurements;

namespace Oilgas.Hardware;

/// <summary>Фіксований набір одиниць обладнання різних типів (демо-«поле»).</summary>
public static class HardwareRoster
{
    public static IReadOnlyList<HardwareUnit> Build() =>
    [
        new(DeviceType.EspPump, 1, Field.North, Well.W12,
        [
            //                       metric                        base  noise  spike%  ±
            new(Measurement.IntakePressure,  80,   8, 0.05, 0.5),
            new(Measurement.MotorTemp,       95,   6, 0.05, 0.3),
            new(Measurement.Vibration,        3,   1, 0.05, 0.6),
            new(Measurement.Rpm,           3500,  80, 0,    0),
        ]),
        new(DeviceType.Wellhead, 1, Field.North, Well.W12,
        [
            new(Measurement.TubingPressure, 150, 10, 0.05, 0.4),
            new(Measurement.CasingPressure,  90,  6, 0.05, 0.4),
            new(Measurement.Temperature,     70,  4, 0.03, 0.35),
        ]),
        new(DeviceType.Separator, 1, Field.North, Well.Cpf,
        [
            new(Measurement.Pressure, 12, 1.5, 0.04, 0.5),
            new(Measurement.Level,    60,   8, 0.04, 0.4),
            new(Measurement.GasFlow, 500,  40, 0,    0),
        ]),
        new(DeviceType.Compressor, 1, Field.North, Well.Cpf,
        [
            new(Measurement.DischargePressure, 40, 4, 0.05, 0.5),
            new(Measurement.Temperature,       85, 6, 0.05, 0.35),
            new(Measurement.Vibration,          4, 1, 0.05, 0.6),
        ]),
    ];
}
