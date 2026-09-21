namespace Oilgas.Ui;

/// <summary>Підключення до MQTT-брокера (секція "Mqtt") — для команд керування залізом.</summary>
public sealed class MqttOptions
{
    public const string SectionName = "Mqtt";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
}
