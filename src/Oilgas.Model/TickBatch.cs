namespace Oilgas.Model;

/// <summary>
/// Пачка тіків з одного пристрою на один момент — форма на дроті (MQTT/RabbitMQ, JSON camelCase).
/// Родовід: <b>TickBatch</b> (пачка) → <see cref="TickSample"/> (елемент) → <see cref="Tick"/> (рядок у БД).
/// Спільні поля (device/field/well/timestamp) — раз на пачку; DbWriter розкладає кожен Sample у Tick.
/// </summary>
public sealed record TickBatch(
    string DeviceId,
    string DeviceType,
    string Field,
    string Well,
    DateTimeOffset Timestamp,
    IReadOnlyList<TickSample> Samples);
