namespace Oilgas.Model;

/// <summary>Тип одиниці обладнання. Закритий набір — джерело правди для всіх шарів.</summary>
public enum DeviceType
{
    EspPump,     // електроцентробіжний насос
    Wellhead,    // гирло свердловини
    Separator,   // сепаратор
    Compressor   // компресор
}
