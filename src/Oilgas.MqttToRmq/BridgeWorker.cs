using System.Buffers;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using RabbitMQ.Client;

namespace Oilgas.MqttToRmq;

/// <summary>
/// Міст Mosquitto → RabbitMQ. Підписується на MQTT-топіки і ПЕРЕКЛАДАЄ байти повідомлення
/// у topic-exchange RabbitMQ, а MQTT-топік кладе в routing key ('/' → '.').
/// Pass-through: жодної десеріалізації — мінімум CPU/алокацій на гарячому шляху
/// (див. problems/mqtt-rabbit-bridge.md).
/// </summary>
public sealed class BridgeWorker(
    IOptions<MqttOptions> mqttOptions,
    IOptions<RabbitOptions> rabbitOptions,
    ILogger<BridgeWorker> logger) : BackgroundService
{
    private readonly MqttOptions _mqtt = mqttOptions.Value;
    private readonly RabbitOptions _rabbit = rabbitOptions.Value;

    private IConnection? _conn;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await ConnectRabbitAsync(ct);

        var factory = new MqttClientFactory();
        var client = factory.CreateMqttClient();

        client.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var body = e.ApplicationMessage.Payload.ToArray();
            var routingKey = topic.Replace('/', '.');
            try
            {
                await _channel!.BasicPublishAsync(_rabbit.Exchange, routingKey, body, cancellationToken: ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Publish to RabbitMQ failed for {Topic}: {Message}", topic, ex.Message);
            }
        };

        var opts = new MqttClientOptionsBuilder()
            .WithTcpServer(_mqtt.Host, _mqtt.Port)
            .WithClientId("oilgas-bridge")
            .Build();

        for (var attempt = 1; !ct.IsCancellationRequested; attempt++)
        {
            try
            {
                await client.ConnectAsync(opts, ct);
                await client.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic(_mqtt.TopicFilter)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build(), ct);
                logger.LogInformation("Bridge up: MQTT {Filter} -> RabbitMQ exchange {Exchange}",
                    _mqtt.TopicFilter, _rabbit.Exchange);
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning("MQTT connect attempt {Attempt} failed: {Message}", attempt, ex.Message);
                try { await Task.Delay(TimeSpan.FromSeconds(3), ct); }
                catch (OperationCanceledException) { return; }
            }
        }

        // тримаємо процес живим; уся робота — в обробнику подій
        try { await Task.Delay(Timeout.Infinite, ct); }
        catch (OperationCanceledException) { /* shutdown */ }

        if (client.IsConnected) await client.DisconnectAsync();
    }

    private async Task ConnectRabbitAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbit.Host,
            Port = _rabbit.Port,
            UserName = _rabbit.User,
            Password = _rabbit.Password,
        };

        for (var attempt = 1; !ct.IsCancellationRequested; attempt++)
        {
            try
            {
                _conn = await factory.CreateConnectionAsync(ct);
                _channel = await _conn.CreateChannelAsync(cancellationToken: ct);
                await _channel.ExchangeDeclareAsync(_rabbit.Exchange, ExchangeType.Topic, durable: true, cancellationToken: ct);
                logger.LogInformation("RabbitMQ connected: {Host}:{Port}", _rabbit.Host, _rabbit.Port);
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning("RabbitMQ connect attempt {Attempt} failed: {Message}", attempt, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(3), ct);
            }
        }
    }

    public override async Task StopAsync(CancellationToken ct)
    {
        if (_channel is not null) await _channel.CloseAsync(ct);
        if (_conn is not null) await _conn.CloseAsync(ct);
        await base.StopAsync(ct);
    }
}
