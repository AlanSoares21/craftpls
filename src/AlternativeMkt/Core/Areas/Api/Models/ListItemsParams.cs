namespace AlternativeMkt.Api.Models;

public class ListItemsParams: StandardPaginationParams
{
    public string? name { get; set; }
    public int? level { get; set; }
    public int? maxLevel { get; set; }
    public int? minLevel { get; set; }
    public int? categoryId { get; set; }
    public bool onlyListItemsWithResources { get; set; } = false;
    public string? culture { get; set; }
}