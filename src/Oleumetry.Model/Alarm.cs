namespace Oleumetry.Model;

/// <summary>Аларм — факт порушення порогу. У v1 незмінний (без квитування).</summary>
public class Alarm
{
    public long Id { get; set; }
    public string DeviceId { get; set; } = "";
    public string Metric { get; set; } = "";
    public double Value { get; set; }
    public Severity Severity { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string Status { get; set; } = "active";
}
