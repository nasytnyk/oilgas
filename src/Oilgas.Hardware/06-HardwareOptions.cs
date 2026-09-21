namespace Oilgas.Hardware;

/// <summary>
/// Налаштування симулятора обладнання (секція "Hardware").
/// </summary>
public sealed class HardwareOptions
{
    public const string SectionName = "Hardware";

    public int IntervalSeconds { get; set; } = 5;
}
