using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Oilgas.Contracts;
using Oilgas.Model;
using Oilgas.Postgres;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Oilgas.DbWriter;

/// <summary>
/// Мікросервіс запису: споживає телеметрію з RabbitMQ і пише її рядками Tick у Postgres (EF Core).
/// Durable queue + manual ack: якщо запис у БД не вдався або консюмер упав — повідомлення
/// повертається в чергу й буде оброблене знову (нічого не губимо на рестарті консюмера).
/// </summary>
public sealed class DbWriterConsumer(
    IOptions<RabbitOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<DbWriterConsumer> logger) : BackgroundService
{
    private readonly RabbitOptions _opt = options.Value;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private IConnection? _conn;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await MigrateWithRetryAsync(ct);

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
                await _channel.QueueDeclareAsync(_opt.Queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
                await _channel.QueueBindAsync(_opt.Queue, _opt.Exchange, _opt.Binding, cancellationToken: ct);
                await _channel.BasicQosAsync(0, _opt.Prefetch, global: false, ct);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += OnMessageAsync;
                await _channel.BasicConsumeAsync(_opt.Queue, autoAck: false, consumer: consumer, cancellationToken: ct);

                logger.LogInformation("DbWriter consuming {Queue} <- {Exchange} ({Binding})", _opt.Queue, _opt.Exchange, _opt.Binding);
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

    private async Task OnMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var msg = JsonSerializer.Deserialize<TelemetryMessage>(ea.Body.Span, Json);
            if (msg is not null && msg.Samples.Count > 0)
            {
                var ticks = msg.Samples.Select(s => new Tick
                {
                    DeviceId = msg.DeviceId,
                    Metric = s.Name,
                    Value = s.Value,
                    Unit = s.Unit,
                    Timestamp = msg.Timestamp,
                });

                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OilgasDbContext>();
                db.Ticks.AddRange(ticks);
                await db.SaveChangesAsync();
            }
            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            logger.LogWarning("Write failed for {Rk}: {Message}", ea.RoutingKey, ex.Message);
            // повертаємо в чергу — спробуємо ще (напр. БД тимчасово недоступна)
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
        }
    }

    private async Task MigrateWithRetryAsync(CancellationToken ct)
    {
        for (var attempt = 1; !ct.IsCancellationRequested; attempt++)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                await scope.ServiceProvider.GetRequiredService<OilgasDbContext>().Database.MigrateAsync(ct);
                logger.LogInformation("Postgres schema ready (migrations applied)");
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning("DB migrate attempt {Attempt} failed: {Message}", attempt, ex.Message);
                try { await Task.Delay(TimeSpan.FromSeconds(3), ct); }
                catch (OperationCanceledException) { return; }
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
