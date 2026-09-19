namespace Oleumetry.Contracts;

/// <summary>
/// Телеметричне повідомлення, що прилітає по MQTT (і далі йде шиною RabbitMQ).
/// DeviceType — рядок (напр. "EspPump"), мапиться на enum Model під час інгесту.
/// JSON серіалізується у camelCase (deviceId, deviceType, timestamp, metrics...).
/// </summary>
public sealed record TelemetryMessage(
    string DeviceId,
    string DeviceType,
    string Field,
    string Well,
    DateTimeOffset Timestamp,
    IReadOnlyList<MetricSample> Metrics);
