namespace Oilgas.Model;

/// <summary>
/// DTO для біндингу з measurements.json (mutable-властивості — вимога ConfigurationBinder).
/// Unit подається enum-назвою ("Bar"). Порогів може не бути (null).
/// </summary>
public sealed class MeasurementDefinition
{
    public string Name { get; set; } = "";
    public Unit Unit { get; set; }
    public double Floor { get; set; }
    public double Ceiling { get; set; }
    public int Decimals { get; set; }
    public ThresholdDefinition? Warning { get; set; }
    public ThresholdDefinition? Critical { get; set; }
}

/// <summary>DTO порогу (min/max nullable) для біндингу.</summary>
public sealed class ThresholdDefinition
{
    public double? Min { get; set; }
    public double? Max { get; set; }
}
