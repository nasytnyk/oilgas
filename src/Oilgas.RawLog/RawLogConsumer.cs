using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Oilgas.RawLog;

/// <summary>
/// Споживач RabbitMQ: створює тимчасову чергу, біндить її до topic-exchange і складає
/// кожне повідомлення в <see cref="RawLogStore"/>. Це перший споживач у fan-out — інші
/// (db, графіки) додаються власними чергами до того ж exchange.
/// </summary>
public sealed class RawLogConsumer(
    IOptions<RabbitOptions> options,
    RawLogStore store,
    ILogger<RawLogConsumer> logger) : BackgroundService
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

                var queue = await _channel.QueueDeclareAsync(cancellationToken: ct); // server-named, exclusive
                await _channel.QueueBindAsync(queue.QueueName, _opt.Exchange, _opt.Binding, cancellationToken: ct);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += (_, ea) =>
                {
                    store.Add(ea.RoutingKey, ea.Body.ToArray());
                    return Task.CompletedTask;
                };
                await _channel.BasicConsumeAsync(queue.QueueName, autoAck: true, consumer: consumer, cancellationToken: ct);

                logger.LogInformation("RawLog consuming {Exchange} ({Binding})", _opt.Exchange, _opt.Binding);
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
