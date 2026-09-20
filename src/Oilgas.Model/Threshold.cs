namespace Oilgas.Model;

/// <summary>
/// Межа спрацювання (для warning/critical). Min/Max nullable — метрика може мати
/// лише верхню межу (тиск), лише нижню, обидві (рівень), або жодної (rpm).
/// </summary>
public sealed record Threshold(double? Min = null, double? Max = null);
