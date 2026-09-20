namespace Oilgas.RawLog;

/// <summary>Підключення до RabbitMQ + topic-exchange (секція "Rabbit").</summary>
public sealed class RabbitOptions
{
    public const string SectionName = "Rabbit";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string User { get; set; } = "oilgas";
    public string Password { get; set; } = "oilgas";
    public string Exchange { get; set; } = "oilgas.telemetry";
    public string Binding { get; set; } = "#";     // усе (rawlog бачить кожне повідомлення)
    public int Capacity { get; set; } = 200;        // скільки останніх тримати в пам'яті
}
