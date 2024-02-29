using System.Text.Json.Serialization;

namespace AlternativeMkt.Db;

public class Attribute
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    [JsonIgnore]
    public List<CraftItemAttribute> CraftItems { get; set; } = new();
}