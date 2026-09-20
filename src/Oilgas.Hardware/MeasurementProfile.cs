using Oilgas.Measurements;

namespace Oilgas.Hardware;

/// <summary>
/// Профіль генерації одного виміру: база + шум, і зрідка — сплеск.
/// Парний до Model.MeasurementBoundary за <see cref="Measurement"/>:
/// profile — ЯК генерувати, boundary — КОЛИ аларм.
/// Одиниця не зберігається тут — вона похідна від метрики (MeasurementCatalog.UnitOf).
/// </summary>
public sealed record MeasurementProfile(
    Measurement Measurement, // яку величину симулюємо
    double Baseline,         // центральне значення
    double Noise,            // амплітуда рівномірного шуму (±Noise)
    double SpikeChance,      // ймовірність сплеску на тік (0..1)
    double SpikePercent);    // величина сплеску як частка, застосовується ± випадково (0.4 = ±40%)
