namespace AlternativeMkt.Api.Models;

public class NewItemData
{
    public string name { get; set; } = "";
    public int? categoryId { get; set; }
    public int? level { get; set; }
    public int? assetId { get; set; }
    public List<ItemAttribute> attributes { get; set; } = new();
}