namespace Oilgas.Hardware;

/// <summary>
/// Профіль генерації метрики: базове значення, шум, шанс і величина сплеску.
/// Парний до Model.MetricBoundary за іменем метрики: profile — ЯК генерувати, boundary — КОЛИ аларм.
/// </summary>
public sealed record MetricProfile(
    string Name, string Unit, double Baseline, double Noise, double SpikeChance, double SpikeDelta);
