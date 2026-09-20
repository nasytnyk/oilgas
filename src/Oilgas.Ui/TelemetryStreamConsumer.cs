using System.Text;
using HotChocolate.Subscriptions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Oilgas.Ui;

/// <summary>
/// Споживач RabbitMQ, що живить GraphQL-subscription: кожне повідомлення з exchange
/// пушиться у топік "telemetry" через ITopicEventSender → долітає до підписаних клієнтів.
/// </summary>
public sealed class TelemetryStreamConsumer(
    IOptions<RabbitOptions> options,
    ITopicEventSender sender,
    ILogger<TelemetryStreamConsumer> logger) : BackgroundService
{
    private readonly RabbitOptions _opt = options.Value;
    private IConnection? _conn;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _opt.Host,
            Port = _opt.Port,
            UserName = _opt.User,
            Password = _opt.Password,
        };

        for (var attempt = 1; !ct.IsCancellationRequested; attempt++)
        {
            try
            {
                _conn = await factory.CreateConnectionAsync(ct);
                _channel = await _conn.CreateChannelAsync(cancellationToken: ct);
                await _channel.ExchangeDeclareAsync(_opt.Exchange, ExchangeType.Topic, durable: true, cancellationToken: ct);
                var queue = await _channel.QueueDeclareAsync(cancellationToken: ct); // ephemeral
                await _channel.QueueBindAsync(queue.QueueName, _opt.Exchange, _opt.Binding, cancellationToken: ct);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    var ev = new LiveEvent(ea.RoutingKey, Encoding.UTF8.GetString(ea.Body.Span), DateTimeOffset.UtcNow);
                    await sender.SendAsync("telemetry", ev, ct);
                };
                await _channel.BasicConsumeAsync(queue.QueueName, autoAck: true, consumer: consumer, cancellationToken: ct);
                logger.LogInformation("WebTier streaming {Exchange} ({Binding}) -> GraphQL subscription", _opt.Exchange, _opt.Binding);
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning("RabbitMQ connect attempt {Attempt} failed: {Message}", attempt, ex.Message);
                try { await Task.Delay(TimeSpan.FromSeconds(3), ct); }
                catch (OperationCanceledException) { return; }
            }
        }

        try { await Task.Delay(Timeout.Infinite, ct); }
        catch (OperationCanceledException) { /* shutdown */ }
    }

    public override async Task StopAsync(CancellationToken ct)
    {
        if (_channel is not null) await _channel.CloseAsync(ct);
        if (_conn is not null) await _conn.CloseAsync(ct);
        await base.StopAsync(ct);
    }
}
