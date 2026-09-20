namespace Oilgas.Model;

/// <summary>
/// Один вимір у пачці TickBatch. Під час інгесту стає рядком Tick у сховищі.
/// </summary>
public sealed record TickSample(string Name, double Value, string Unit);
