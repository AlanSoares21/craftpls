using AlternativeMkt.Api.Models;

namespace AlternativeMkt.Models;

public class ListPricesParams : StandardPaginationParams
{
    public bool? resourcesChanged  { get; set; }
    public Guid? manufacturerId { get; set; }
    public int? itemId { get; set; }
    public string? itemName { get; set; }
    public int? itemCategory { get; set; }
    public int? itemMaxLevel { get; set; }
    public int? itemMinLevel { get; set; }
    public int? resourcesOf { get; set; }
    public byte? serverId { get; set; }
    public bool orderByCraftPrice { get; set; } = false;
    public bool orderByCreatedDate { get; set; } = false;
    public bool onlyListItemsWithResources { get; set; } = false;
}