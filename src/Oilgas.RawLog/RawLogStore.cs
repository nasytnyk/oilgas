using System.Collections.Concurrent;
using System.Text;

namespace Oilgas.RawLog;

/// <summary>Кільцевий буфер останніх повідомлень у пам'яті (втрачається на рестарті — це лог, не сховище).</summary>
public sealed class RawLogStore(int capacity)
{
    public sealed record Entry(string Topic, string Payload, DateTimeOffset At);

    private readonly ConcurrentQueue<Entry> _items = new();
    private long _seq;

    public void Add(string routingKey, byte[] body)
    {
        var payload = Encoding.UTF8.GetString(body);
        _items.Enqueue(new Entry(routingKey, payload, DateTimeOffset.UtcNow));
        Interlocked.Increment(ref _seq);
        while (_items.Count > capacity && _items.TryDequeue(out _)) { }
    }

    /// <summary>Останні повідомлення, найновіші зверху.</summary>
    public IReadOnlyList<Entry> Snapshot() => _items.Reverse().ToArray();
}
