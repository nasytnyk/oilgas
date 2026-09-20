using Oilgas.Measurements;

namespace Oilgas.Hardware;

/// <summary>
/// Фіксований набір одиниць обладнання (демо-«поле»). Тут — лише per-device тюнінг симуляції;
/// усе інтринсивне метриці (одиниця, межі, пороги) живе в <see cref="Measurement"/>.
/// У проді цей набір приходив би з БД/конфігу.
/// </summary>
public static class HardwareRoster
{
    public static IReadOnlyList<HardwareUnit> Build() =>
    [
        new(DeviceType.EspPump, 1, Field.North, Well.W12,
        [
            new(Measurement.IntakePressure, Baseline: 80,   Noise: 8,  SpikeChance: 0.05, SpikePercent: 0.5),
            new(Measurement.MotorTemp,      Baseline: 95,   Noise: 6,  SpikeChance: 0.05, SpikePercent: 0.3),
            new(Measurement.Vibration,      Baseline: 3,    Noise: 1,  SpikeChance: 0.05, SpikePercent: 0.6),
            new(Measurement.Rpm,            Baseline: 3500, Noise: 80, SpikeChance: 0,    SpikePercent: 0),
        ]),
        new(DeviceType.Wellhead, 1, Field.North, Well.W12,
        [
            new(Measurement.TubingPressure, Baseline: 150, Noise: 10, SpikeChance: 0.05, SpikePercent: 0.4),
            new(Measurement.CasingPressure, Baseline: 90,  Noise: 6,  SpikeChance: 0.05, SpikePercent: 0.4),
            new(Measurement.Temperature,    Baseline: 70,  Noise: 4,  SpikeChance: 0.03, SpikePercent: 0.35),
        ]),
        new(DeviceType.Separator, 1, Field.North, Well.Cpf,
        [
            new(Measurement.Pressure, Baseline: 12,  Noise: 1.5, SpikeChance: 0.04, SpikePercent: 0.5),
            new(Measurement.Level,    Baseline: 60,  Noise: 8,   SpikeChance: 0.04, SpikePercent: 0.4),
            new(Measurement.GasFlow,  Baseline: 500, Noise: 40,  SpikeChance: 0,    SpikePercent: 0),
        ]),
        new(DeviceType.Compressor, 1, Field.North, Well.Cpf,
        [
            new(Measurement.DischargePressure, Baseline: 40, Noise: 4, SpikeChance: 0.05, SpikePercent: 0.5),
            new(Measurement.Temperature,       Baseline: 85, Noise: 6, SpikeChance: 0.05, SpikePercent: 0.35),
            new(Measurement.Vibration,         Baseline: 4,  Noise: 1, SpikeChance: 0.05, SpikePercent: 0.6),
        ]),
    ];
}
