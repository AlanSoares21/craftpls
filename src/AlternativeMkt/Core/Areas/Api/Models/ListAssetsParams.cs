namespace AlternativeMkt.Api.Models;

public class ListAssetsParams: StandardPaginationParams
{
    public string? endpoint { get; set; }
    public string? itemName { get; set; }
    public bool? unusedAssets { get; set; }
}