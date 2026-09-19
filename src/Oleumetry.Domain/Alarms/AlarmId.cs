namespace Oleumetry.Domain.Alarms;

/// <summary>
/// Ідентифікатор аларму. Генерується доменом при створенні факту
/// (Guid — не потребує звернення до БД, аларм можна створити «в пам'яті»).
/// </summary>
public readonly record struct AlarmId(Guid Value)
{
    public static AlarmId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
