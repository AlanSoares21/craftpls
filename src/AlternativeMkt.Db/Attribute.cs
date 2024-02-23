namespace AlternativeMkt.Db;

public class Attribute
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<CraftItemAttribute> CraftItems { get; set; } = new();
}