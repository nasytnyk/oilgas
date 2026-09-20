using System.Collections.Concurrent;
using System.Text;

namespace Oilgas.Measurements;

/// <summary>
/// Єдине правило «enum → рядок для дроту/БД»: назва значення у snake_case
/// (IntakePressure → "intake_pressure", Online → "online", W12 → "w12").
/// Замінює купу рукописних switch-мап. Виняток — <see cref="MeasurementCatalog.Symbol"/>,
/// де рядок не виводиться з назви (Celsius → "C").
/// </summary>
public static class EnumWire
{
    private static readonly ConcurrentDictionary<Enum, string> Cache = new();

    public static string Wire<TEnum>(this TEnum value) where TEnum : struct, Enum =>
        Cache.GetOrAdd(value, static v => ToSnake(v.ToString()!));

    private static string ToSnake(string name)
    {
        var sb = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0) sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
