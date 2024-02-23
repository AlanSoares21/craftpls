namespace AlternativeMkt.Db;

public class CraftItemAttribute
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public CraftItem Item { get; set; } = null!;
    public int AttributeId { get; set; }
    public Attribute Attribute { get; set; } = null!;
    public float Value { get; set; }
}