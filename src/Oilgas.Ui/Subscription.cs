using HotChocolate.Subscriptions;

namespace Oilgas.Ui;

/// <summary>GraphQL subscription — жива телеметрія з RabbitMQ (топік "telemetry").</summary>
public sealed class Subscription
{
    [Subscribe]
    [Topic("telemetry")]
    public LiveEvent OnTelemetry([EventMessage] LiveEvent ev) => ev;
}
