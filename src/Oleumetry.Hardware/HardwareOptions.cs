namespace Oleumetry.Hardware;

/// <summary>Налаштування обладнання-емулятора (секція "Hardware"; дефолти — локальний EMQX).</summary>
public sealed class HardwareOptions
{
    public const string SectionName = "Hardware";

    public string BrokerHost { get; set; } = "localhost";
    public int BrokerPort { get; set; } = 1883;
    public int IntervalSeconds { get; set; } = 5;
}
