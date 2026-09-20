using Oilgas.Measurements;

namespace Oilgas.Hardware;

/// <summary>
/// Per-device тюнінг симуляції однієї метрики: база + шум, і зрідка — сплеск.
/// Усе інтринсивне (одиниця, фізичні межі, точність, алармові пороги) живе в
/// <see cref="Measurement"/>; тут — лише те, що залежить від конкретного пристрою.
/// </summary>
public sealed record MeasurementProfile(
    Measurement Measurement, // яку величину симулюємо
    double Baseline,         // центральне значення
    double Noise,            // амплітуда рівномірного шуму (±Noise)
    double SpikeChance,      // ймовірність сплеску на тік (0..1)
    double SpikePercent);    // величина сплеску як частка, застосовується ± випадково (0.4 = ±40%)
