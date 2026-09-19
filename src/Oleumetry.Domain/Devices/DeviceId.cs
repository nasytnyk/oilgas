namespace Oleumetry.Domain.Devices;

/// <summary>
/// Типізований ідентифікатор пристрою (напр. "esp-001").
/// readonly record struct: рівність за значенням, незмінний, без heap-алокації.
/// </summary>
public readonly record struct DeviceId
{
    public string Value { get; }

    public DeviceId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("DeviceId не може бути порожнім.", nameof(value));
        Value = value;
    }

    public override string ToString() => Value;
}
