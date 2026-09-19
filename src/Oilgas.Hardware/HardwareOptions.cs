namespace Oilgas.Hardware;

/// <summary>
/// Налаштування обладнання-емулятора (секція "Hardware").
/// Дефолти — локальний Mosquitto без TLS; у хмарі хост/порт задаються через env (Hardware__*).
/// </summary>
public sealed class HardwareOptions
{
    public const string SectionName = "Hardware";

    public string BrokerHost { get; set; } = "localhost";
    public int BrokerPort { get; set; } = 1883;

    public bool UseTls { get; set; }              // TLS-порт брокера (:8883), якщо колись знадобиться
    public string? Username { get; set; }
    public string? Password { get; set; }

    public int IntervalSeconds { get; set; } = 5;
}
