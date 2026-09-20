namespace Oilgas.WebTier;

/// <summary>Жива подія з RabbitMQ (для subscription): routing key + сирий payload.</summary>
public sealed record LiveEvent(string Topic, string Payload, DateTimeOffset At);

/// <summary>Стан пристрою, виведений із таблиці Tick (останній замір).</summary>
public sealed record DeviceState(string DeviceId, DateTimeOffset LastSeen)
{
    /// <summary>Онлайн, якщо останній тік був нещодавно.</summary>
    public bool Online => DateTimeOffset.UtcNow - LastSeen < TimeSpan.FromSeconds(30);
}

/// <summary>Діагностичні лічильники.</summary>
public sealed record Stats(long TickCount, int DeviceCount);
