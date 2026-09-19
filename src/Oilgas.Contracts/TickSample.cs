namespace Oilgas.Contracts;

/// <summary>
/// Один вимір у складі TelemetryMessage. Кожен TickSample під час інгесту стає рядком Tick у сховищі.
/// </summary>
public sealed record TickSample(string Name, double Value, string Unit);
