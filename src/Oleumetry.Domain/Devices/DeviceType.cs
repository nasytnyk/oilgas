namespace Oleumetry.Domain.Devices;

/// <summary>
/// Тип обладнання. Визначає набір метрик і порогів (через DeviceTypeProfile).
/// </summary>
public enum DeviceType
{
    EspPump,     // електроцентробіжний насос
    Wellhead,    // гирло свердловини
    Separator,   // сепаратор
    Compressor   // компресор
}
