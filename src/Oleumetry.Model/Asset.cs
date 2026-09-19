namespace Oleumetry.Model;

/// <summary>Актив/свердловина — верхній рівень реєстру обладнання.</summary>
public class Asset
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Field { get; set; } = "";
    public string Well { get; set; } = "";

    // Навігація для EF (один актив — багато пристроїв)
    public List<Device> Devices { get; set; } = new();
}
