namespace Oilgas.Measurements;

/// <summary>
/// Що саме вимірюємо. Закритий набір — джерело правди, спільне для Hardware, Contracts, Model.
/// Одиниця кожної метрики фіксована й береться з <see cref="MeasurementCatalog.UnitOf"/>,
/// рядок для дроту/БД — з <see cref="MeasurementCatalog.Wire"/>.
/// </summary>
public enum Measurement
{
    IntakePressure,
    MotorTemp,
    Vibration,
    Rpm,
    TubingPressure,
    CasingPressure,
    Temperature,
    Pressure,
    Level,
    GasFlow,
    DischargePressure,
}
