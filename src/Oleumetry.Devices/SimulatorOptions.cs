namespace Oleumetry.Devices;

/// <summary>Налаштування симулятора (з конфігу секції "Simulator"; дефолти — локальний EMQX).</summary>
public sealed class SimulatorOptions
{
    public const string SectionName = "Simulator";

    public string BrokerHost { get; set; } = "localhost";
    public int BrokerPort { get; set; } = 1883;
    public int IntervalSeconds { get; set; } = 5;
}
