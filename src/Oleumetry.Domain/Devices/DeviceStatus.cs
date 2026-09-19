namespace Oleumetry.Domain.Devices;

/// <summary>Стан зв'язку пристрою + відколи він триває (для віджета MQTT-статусу).</summary>
public sealed record DeviceStatus(DeviceState State, DateTimeOffset Since)
{
    public bool IsOnline => State == DeviceState.Online;

    public static DeviceStatus Online(DateTimeOffset at) => new(DeviceState.Online, at);
    public static DeviceStatus Offline(DateTimeOffset at) => new(DeviceState.Offline, at);
}
