namespace Oilgas.Measurements;

/// <summary>Рядкові представлення пристрою для дроту/БД. Enum усередині — рядок зовні.</summary>
public static class DeviceCatalog
{
    /// <summary>Тип пристрою для дроту/БД (PascalCase: EspPump, Wellhead, ...).</summary>
    public static string Wire(this DeviceType t) => t.ToString();

    /// <summary>Стан пристрою для status-топіка (online/offline).</summary>
    public static string Wire(this DeviceState s) => s switch
    {
        DeviceState.Online => "online",
        DeviceState.Offline => "offline",
        _ => throw new ArgumentOutOfRangeException(nameof(s), s, "Unknown device state"),
    };
}
