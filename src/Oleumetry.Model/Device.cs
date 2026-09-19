namespace Oleumetry.Model;

/// <summary>Одиниця обладнання. Id — природний ключ із MQTT (напр. "esp-001").</summary>
public class Device
{
    public string Id { get; set; } = "";
    public int AssetId { get; set; }
    public Asset? Asset { get; set; }              // навігація

    public DeviceType Type { get; set; }
    public string Name { get; set; } = "";
    public string? Serial { get; set; }

    public bool IsOnline { get; set; }
    public DateTimeOffset? LastSeenAt { get; set; }
}
