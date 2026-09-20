namespace Oilgas.MqttToRmq;

/// <summary>Підключення до MQTT-брокера (секція "Mqtt").</summary>
public sealed class MqttOptions
{
    public const string SectionName = "Mqtt";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
    public string TopicFilter { get; set; } = "oilgas/#";
}

/// <summary>Підключення до RabbitMQ + topic-exchange (секція "Rabbit").</summary>
public sealed class RabbitOptions
{
    public const string SectionName = "Rabbit";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string User { get; set; } = "oilgas";
    public string Password { get; set; } = "oilgas";
    public string Exchange { get; set; } = "oilgas.telemetry";
}
