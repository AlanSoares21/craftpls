namespace AlternativeMkt.Api.Models;

public class ListItemsParams: StandardPaginationParams
{
    public string? name { get; set; }
    public int? level { get; set; }
    public int? maxLevel { get; set; }
    public int? minLevel { get; set; }
    public int? categoryId { get; set; }
}