namespace Oleumetry.Devices;

/// <summary>Налаштування пристроїв-емуляторів (з конфігу секції "Devices"; дефолти — локальний EMQX).</summary>
public sealed class DeviceOptions
{
    public const string SectionName = "Devices";

    public string BrokerHost { get; set; } = "localhost";
    public int BrokerPort { get; set; } = 1883;
    public int IntervalSeconds { get; set; } = 5;
}
