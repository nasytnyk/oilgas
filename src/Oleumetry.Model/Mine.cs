namespace Oleumetry.Model;

/// <summary>Копальня/актив — верхній рівень реєстру обладнання.</summary>
public class Mine
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Field { get; set; } = "";
    public string Well { get; set; } = "";

    // Навігація для EF (одна копальня — багато пристроїв)
    public List<Device> Devices { get; set; } = new();
}
