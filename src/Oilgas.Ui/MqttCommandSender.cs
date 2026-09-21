using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;

namespace Oilgas.Ui;

/// <summary>
/// Публікує команди керування залізом у MQTT (топік oilgas/control/telemetry, "on"/"off").
/// Так Ui вмикає/вимикає телеметрію без HTTP до Hardware — усе через ту саму MQTT-шину.
/// Singleton: конектиться раз (лениво) і тримає з'єднання.
/// </summary>
public sealed class MqttCommandSender(IOptions<MqttOptions> options) : IAsyncDisposable
{
    public const string CommandTopic = "oilgas/control/telemetry";

    private readonly MqttOptions _mqtt = options.Value;
    private readonly IMqttClient _client = new MqttClientFactory().CreateMqttClient();
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task SetTelemetryAsync(bool on, CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(CommandTopic)
            .WithPayload(on ? "on" : "off")
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();
        await _client.PublishAsync(message, ct);
    }

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        if (_client.IsConnected) return;
        await _gate.WaitAsync(ct);
        try
        {
            if (!_client.IsConnected)
            {
                var opts = new MqttClientOptionsBuilder()
                    .WithTcpServer(_mqtt.Host, _mqtt.Port)
                    .WithClientId("oilgas-ui-cmd")
                    .Build();
                await _client.ConnectAsync(opts, ct);
            }
        }
        finally { _gate.Release(); }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client.IsConnected) await _client.DisconnectAsync();
        _client.Dispose();
    }
}
