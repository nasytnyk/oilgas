namespace Oleumetry.Domain.Assets;

/// <summary>Розташування активу: родовище + свердловина.</summary>
public sealed record Location
{
    public string Field { get; }
    public string Well { get; }

    public Location(string field, string well)
    {
        if (string.IsNullOrWhiteSpace(field))
            throw new ArgumentException("Field не може бути порожнім.", nameof(field));
        if (string.IsNullOrWhiteSpace(well))
            throw new ArgumentException("Well не може бути порожнім.", nameof(well));
        Field = field;
        Well = well;
    }
}
