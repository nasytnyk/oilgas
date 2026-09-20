namespace Oilgas.Ui;

/// <summary>Підключення до RabbitMQ (секція "Rabbit"). WebTier — ще один споживач exchange.</summary>
public sealed class RabbitOptions
{
    public const string SectionName = "Rabbit";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string User { get; set; } = "oilgas";
    public string Password { get; set; } = "oilgas";
    public string Exchange { get; set; } = "oilgas.telemetry";
    public string Binding { get; set; } = "#";
}
