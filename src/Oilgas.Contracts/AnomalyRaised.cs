namespace Oilgas.Contracts;

/// <summary>
/// Інтеграційна подія: порушено межу (Boundary). Емітиться під час оцінки телеметрії,
/// йде шиною RabbitMQ до споживачів (persist, realtime → UI).
/// Severity — рядок ("Warning" | "Critical").
/// </summary>
public sealed record AnomalyRaised(
    string DeviceId,
    string Metric,
    double Value,
    string Severity,
    DateTimeOffset Timestamp);
