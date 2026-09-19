namespace Oleumetry.Domain.Assets;

/// <summary>
/// Типізований ідентифікатор активу/свердловини (напр. "north-w12").
/// Рядок — щоб id був людиночитабельним; за потреби легко змінити тип.
/// </summary>
public readonly record struct AssetId
{
    public string Value { get; }

    public AssetId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("AssetId не може бути порожнім.", nameof(value));
        Value = value;
    }

    public override string ToString() => Value;
}
