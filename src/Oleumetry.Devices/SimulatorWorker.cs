using System.Text.Json;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;

namespace Oleumetry.Devices;

/// <summary>
/// Фонова служба: тримає один MQTT-клієнт на кожен емульований пристрій
/// (щоб LWT давав per-device offline), і циклічно публікує телеметрію в EMQX.
/// </summary>
public sealed class SimulatorWorker(
    IOptions<SimulatorOptions> options,
    ILogger<SimulatorWorker> logger) : BackgroundService
{
    private readonly SimulatorOptions _opt = options.Value;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly List<(SimulatedDevice Device, IMqttClient Client)> _clients = [];

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var factory = new MqttClientFactory();

        // 1) конект кожного пристрою окремим клієнтом + LWT (offline, retained) + online-статус
        foreach (var device in DeviceRoster.Build())
        {
            var client = factory.CreateMqttClient();
            var will = JsonSerializer.SerializeToUtf8Bytes(
                device.BuildStatus("offline", DateTimeOffset.UtcNow), Json);

            var opts = new MqttClientOptionsBuilder()
                .WithTcpServer(_opt.BrokerHost, _opt.BrokerPort)
                .WithClientId($"oleumetry-sim-{device.Id}")
                .WithWillTopic(device.StatusTopic)
                .WithWillPayload(will)
                .WithWillRetain(true)
                .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await client.ConnectAsync(opts, ct);
            await PublishAsync(client, device.StatusTopic,
                device.BuildStatus("online", DateTimeOffset.UtcNow), retain: true, ct);

            _clients.Add((device, client));
            logger.LogInformation("Device {DeviceId} online", device.Id);
        }

        // 2) цикл публікації телеметрії
        var period = TimeSpan.FromSeconds(_opt.IntervalSeconds);
        while (!ct.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var (device, client) in _clients)
                await PublishAsync(client, device.TelemetryTopic, device.BuildTelemetry(now), retain: false, ct);

            logger.LogInformation("Published telemetry for {Count} devices", _clients.Count);
            try { await Task.Delay(period, ct); }
            catch (OperationCanceledException) { break; }
        }
    }

    private static async Task PublishAsync<T>(
        IMqttClient client, string topic, T payload, bool retain, CancellationToken ct)
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
        // м'яка зупинка: явний offline + дисконект (LWT — запасний варіант при краху)
        foreach (var (device, client) in _clients)
        {
            try
            {
                if (client.IsConnected)
                {
                    await PublishAsync(client, device.StatusTopic,
                        device.BuildStatus("offline", DateTimeOffset.UtcNow), retain: true, CancellationToken.None);
                    await client.DisconnectAsync();
                }
            }
            catch { /* ігноруємо помилки під час завершення */ }
            client.Dispose();
        }
        await base.StopAsync(ct);
    }
}
