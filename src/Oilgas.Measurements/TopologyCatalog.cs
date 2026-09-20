namespace Oilgas.Measurements;

/// <summary>Рядкові ідентифікатори топології (родовище/свердловина) для дроту/БД.</summary>
public static class TopologyCatalog
{
    /// <summary>Ідентифікатор родовища для дроту/топіка.</summary>
    public static string Wire(this Field f) => f switch
    {
        Field.North => "north",
        _ => throw new ArgumentOutOfRangeException(nameof(f), f, "Unknown field"),
    };

    /// <summary>Ідентифікатор свердловини/майданчика для дроту/топіка.</summary>
    public static string Wire(this Well w) => w switch
    {
        Well.W12 => "w-12",
        Well.Cpf => "cpf",
        _ => throw new ArgumentOutOfRangeException(nameof(w), w, "Unknown well"),
    };
}
