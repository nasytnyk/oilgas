namespace Oilgas.Hardware;

/// <summary>
/// Triggered from:
/// - ControlListener
/// - TelemetryWorker
/// </summary>
public sealed class TelemetryToggle
{
    private volatile bool _on;

    public bool IsOn => _on;
    public void Start() => _on = true;
    public void Stop() => _on = false;
}
