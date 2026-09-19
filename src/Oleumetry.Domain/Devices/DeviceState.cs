namespace Oleumetry.Domain.Devices;

/// <summary>
/// Стан зв'язку пристрою. Визначається через MQTT (LWT + retained status-топік).
/// </summary>
public enum DeviceState
{
    Online,
    Offline
}
