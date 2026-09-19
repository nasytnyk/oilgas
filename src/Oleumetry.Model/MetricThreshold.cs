namespace Oleumetry.Model;

/// <summary>
/// Пороги для метрики певного типу обладнання (реф-дані в БД).
/// Межі nullable: null = немає порогу з цього боку (односторонній аларм).
/// </summary>
public class MetricThreshold
{
    public int Id { get; set; }
    public DeviceType DeviceType { get; set; }
    public string Metric { get; set; } = "";
    public string Unit { get; set; } = "";

    public double? WarningMin { get; set; }
    public double? WarningMax { get; set; }
    public double? CriticalMin { get; set; }
    public double? CriticalMax { get; set; }
}
