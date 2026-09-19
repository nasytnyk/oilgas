namespace Oleumetry.Domain.Alarms;

/// <summary>
/// Серйозність. Info — інформаційні події (зміна статусу, повернення в норму);
/// Warning / Critical — порушення порогів. Порядок відображає зростання серйозності.
/// </summary>
public enum Severity
{
    Info,
    Warning,
    Critical
}
