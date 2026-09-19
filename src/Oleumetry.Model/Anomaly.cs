namespace Oleumetry.Model;

/// <summary>Аномалія — факт порушення межі (Boundary). У v1 незмінна (без квитування).</summary>
public class Anomaly
{
    public long Id { get; set; }
    public string DeviceId { get; set; } = "";
    public string Metric { get; set; } = "";
    public double Value { get; set; }
    public Severity Severity { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string Status { get; set; } = "active";
}
