using System.Text.Json;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;

namespace Oleumetry.Hardware;

/// <summary>
/// Фонова служба: конектить кожну одиницю обладнання окремим MQTT-клієнтом (LWT per-device),
/// і публікує телеметрію ЛИШЕ коли TelemetrySwitch увімкнено. Дефолт OFF — не флудить.
/// </summary>
public sealed class TelemetryWorker(
    IOptions<HardwareOptions> options,
    TelemetrySwitch telemetry,
    ILogger<TelemetryWorker> logger) : BackgroundService
{
    private readonly HardwareOptions _opt = options.Value;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly List<(HardwareUnit Unit, IMqttClient Client)> _clients = [];

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var factory = new MqttClientFactory();

        // конект усіх одиниць + LWT; початковий retained-статус offline (дефолт OFF)
        foreach (var unit in HardwareRoster.Build())
        {
            var client = factory.CreateMqttClient();
            var will = JsonSerializer.SerializeToUtf8Bytes(unit.BuildStatus("offline", DateTimeOffset.UtcNow), Json);

            var opts = new MqttClientOptionsBuilder()
                .WithTcpServer(_opt.BrokerHost, _opt.BrokerPort)
                .WithClientId($"oleumetry-hw-{unit.Id}")
                .WithWillTopic(unit.StatusTopic)
                .WithWillPayload(will)
                .WithWillRetain(true)
                .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await client.ConnectAsync(opts, ct);
            await PublishAsync(client, unit.StatusTopic, unit.BuildStatus("offline", DateTimeOffset.UtcNow), retain: true, ct);
            _clients.Add((unit, client));
        }
        logger.LogInformation("Hardware connected: {Count} units (telemetry OFF by default)", _clients.Count);

        var period = TimeSpan.FromSeconds(_opt.IntervalSeconds);
        var prevOn = false;

        while (!ct.IsCancellationRequested)
        {
            var on = telemetry.IsOn;

            if (on != prevOn) // при перемиканні — оновлюємо retained-статус пристроїв
            {
                var status = on ? "online" : "offline";
                foreach (var (unit, client) in _clients)
                    await PublishAsync(client, unit.StatusTopic, unit.BuildStatus(status, DateTimeOffset.UtcNow), retain: true, ct);
                logger.LogInformation("Telemetry {State}", on ? "STARTED" : "STOPPED");
                prevOn = on;
            }

            if (on)
            {
                var now = DateTimeOffset.UtcNow;
                foreach (var (unit, client) in _clients)
                    await PublishAsync(client, unit.TelemetryTopic, unit.BuildTelemetry(now), retain: false, ct);
                logger.LogInformation("Published telemetry for {Count} units", _clients.Count);
            }

            try { await Task.Delay(period, ct); }
            catch (OperationCanceledException) { break; }
        }
    }

    private static async Task PublishAsync<T>(IMqttClient client, string topic, T payload, bool retain, CancellationToken ct)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, Json);
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(bytes)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(retain)
            .Build();
        await client.PublishAsync(message, ct);
    }

    public override async Task StopAsync(CancellationToken ct)
    {
        foreach (var (unit, client) in _clients)
        {
            try
            {
                if (client.IsConnected)
                {
                    await PublishAsync(client, unit.StatusTopic, unit.BuildStatus("offline", DateTimeOffset.UtcNow), retain: true, CancellationToken.None);
                    await client.DisconnectAsync();
                }
            }
            catch { /* ігноруємо помилки під час завершення */ }
            client.Dispose();
        }
        await base.StopAsync(ct);
    }
}
