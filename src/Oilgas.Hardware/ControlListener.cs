using System.Buffers;
using System.Text;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;

namespace Oilgas.Hardware;

/// <summary>
/// Слухає командний MQTT-топік і перемикає <see cref="TelemetryToggle"/>.
/// Керування приходить з Ui (публікує "on"/"off") — тому Hardware не має HTTP взагалі.
/// </summary>
public sealed class ControlListener(
    IOptions<MqttOptions> options,
    TelemetryToggle toggle,
    ILogger<ControlListener> logger) : BackgroundService
{
    public const string CommandTopic = "control/telemetry";
    private readonly MqttOptions _mqtt = options.Value;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var client = new MqttClientFactory().CreateMqttClient();

        client.ApplicationMessageReceivedAsync += e =>
        {
            var cmd = Encoding.UTF8.GetString(e.ApplicationMessage.Payload.ToArray()).Trim().ToLowerInvariant();
            if (cmd == "on") toggle.Start();
            else if (cmd == "off") toggle.Stop();
            logger.LogInformation("Control command: {Cmd}", cmd);
            return Task.CompletedTask;
        };

        var opts = new MqttClientOptionsBuilder()
            .WithTcpServer(_mqtt.Host, _mqtt.Port)
            .WithClientId("oilgas-hw-control")
            .Build();

        for (var attempt = 1; !ct.IsCancellationRequested; attempt++)
        {
            try
            {
                await client.ConnectAsync(opts, ct);
                await client.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic(CommandTopic)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build(), ct);
                logger.LogInformation("Control listening on {Topic}", CommandTopic);
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Control connect attempt {Attempt} failed: {Message}", attempt, ex.Message);
                try { await Task.Delay(TimeSpan.FromSeconds(3), ct); }
                catch (OperationCanceledException) { return; }
            }
        }

        try { await Task.Delay(Timeout.Infinite, ct); }
        catch (OperationCanceledException) { /* shutdown */ }

        if (client.IsConnected) await client.DisconnectAsync();
    }
}
