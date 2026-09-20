namespace Oilgas.DbWriter;

/// <summary>Підключення до RabbitMQ + topic-exchange (секція "Rabbit").</summary>
public sealed class RabbitOptions
{
    public const string SectionName = "Rabbit";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string User { get; set; } = "oilgas";
    public string Password { get; set; } = "oilgas";
    public string Exchange { get; set; } = "oilgas.telemetry";
    public string Queue { get; set; } = "oilgas.db";              // durable — переживає рестарт консюмера
    public string Binding { get; set; } = "oilgas.#.telemetry";   // лише телеметрія
    public ushort Prefetch { get; set; } = 20;
}
