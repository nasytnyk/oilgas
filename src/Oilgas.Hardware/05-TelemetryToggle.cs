namespace Oilgas.Hardware;

/// <summary>
/// Спільний перемикач потоку телеметрії (on/off). Дефолт — OFF (не флудить).
/// Керується HTTP-ендпоінтами /start і /stop; читається фоновим TelemetryWorker.
/// </summary>
public sealed class TelemetryToggle
{
    private volatile bool _on;

    public bool IsOn => _on;
    public void Start() => _on = true;
    public void Stop() => _on = false;
}
