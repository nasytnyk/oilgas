namespace Oleumetry.Domain.Telemetry;

/// <summary>
/// Один вимір телеметрії — факт: метрика, значення, одиниця, час.
/// Позиційний record: чистий носій даних (правдоподібність перевіряє MetricDefinition окремо).
/// </summary>
public sealed record Measurement(string MetricName, double Value, string Unit, DateTimeOffset Timestamp);
