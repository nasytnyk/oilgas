namespace Oleumetry.Contracts;

/// <summary>Один вимір у складі телеметричного повідомлення: назва метрики, значення, одиниця.</summary>
public sealed record MetricSample(string Name, double Value, string Unit);
