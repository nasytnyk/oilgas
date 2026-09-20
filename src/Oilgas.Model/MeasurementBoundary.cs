namespace Oilgas.Model;

/// <summary>
/// Pair to MeasurementProfile
/// </summary>
public class MeasurementBoundary
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
