namespace Oilgas.Hardware;

/// <summary>
/// Підключення до MQTT-брокера (секція "Mqtt").
/// Дефолт — локальний Mosquitto; у хмарі Host задається через env (Mqtt__Host).
/// </summary>
public sealed class MqttOptions
{
    public const string SectionName = "Mqtt";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
}
