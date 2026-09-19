namespace Oilgas.Model;

/// <summary>
/// Пороги для метрики певного типу обладнання (реф-дані в БД).
/// Парна до Hardware.MetricProfile за іменем метрики: profile — ЯК генерувати, boundary — КОЛИ аларм.
/// null з боку = немає межі з цього боку (односторонній аларм).
/// </summary>
public class MetricBoundary
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
