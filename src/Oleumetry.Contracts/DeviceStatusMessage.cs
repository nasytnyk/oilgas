namespace Oleumetry.Contracts;

/// <summary>
/// Повідомлення про стан пристрою (MQTT status-топік: retained + LWT).
/// Status: "online" | "offline".
/// </summary>
public sealed record DeviceStatusMessage(
    string DeviceId,
    string Status,
    DateTimeOffset Timestamp);
