namespace Oilgas.Hardware;

/// <summary>
/// Налаштування обладнання-емулятора (секція "Hardware").
/// Дефолти — локальний EMQX без TLS; у хмарі задаються TLS + логін через env/секрети.
/// </summary>
public sealed class HardwareOptions
{
    public const string SectionName = "Hardware";

    public string BrokerHost { get; set; } = "localhost";
    public int BrokerPort { get; set; } = 1883;

    public bool UseTls { get; set; }              // хмарний EMQX Serverless вимагає TLS (:8883)
    public string? Username { get; set; }
    public string? Password { get; set; }

    public int IntervalSeconds { get; set; } = 5;
}
