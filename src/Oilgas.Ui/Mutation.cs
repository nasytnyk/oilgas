namespace Oilgas.Ui;

/// <summary>GraphQL mutations — керування пайплайном (команда в MQTT).</summary>
public sealed class Mutation
{
    /// <summary>Увімкнути/вимкнути телеметрію: публікує "on"/"off" у командний MQTT-топік.</summary>
    public async Task<bool> SetTelemetry(bool on, MqttCommandSender sender, CancellationToken ct)
    {
        await sender.SetTelemetryAsync(on, ct);
        return true;
    }
}
