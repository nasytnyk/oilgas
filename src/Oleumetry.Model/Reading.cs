namespace Oleumetry.Model;

/// <summary>Один вимір телеметрії (факт). Пишеться потоком, читається для графіків/історії.</summary>
public class Reading
{
    public long Id { get; set; }
    public string DeviceId { get; set; } = "";
    public string Metric { get; set; } = "";
    public double Value { get; set; }
    public string Unit { get; set; } = "";
    public DateTimeOffset Timestamp { get; set; }
}
