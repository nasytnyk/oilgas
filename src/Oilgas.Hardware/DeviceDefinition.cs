namespace Oilgas.Hardware;

/// <summary>
/// DTO для біндингу з hardware.json (mutable — вимога ConfigurationBinder).
/// Топологія — рядки; метрика профілю посилається на wire з measurements.json.
/// </summary>
public sealed class DeviceDefinition
{
    public string Type { get; set; } = "";
    public int Number { get; set; }
    public string Field { get; set; } = "";
    public string Well { get; set; } = "";
    public List<ProfileDefinition> Profiles { get; set; } = [];
}

/// <summary>DTO профілю симуляції однієї метрики на пристрої.</summary>
public sealed class ProfileDefinition
{
    public string Measurement { get; set; } = "";
    public double Baseline { get; set; }
    public double Noise { get; set; }
    public double SpikeChance { get; set; }
    public double SpikePercent { get; set; }
}
