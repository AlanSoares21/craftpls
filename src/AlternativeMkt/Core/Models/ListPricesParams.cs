using AlternativeMkt.Api.Models;

namespace AlternativeMkt.Models;

public class ListPricesParams : StandardPaginationParams
{
    public bool? resourcesChanged  { get; set; }
    public Guid? manufacturerId { get; set; }
    public int? itemId { get; set; }
    public int? resourcesOf { get; set; }
    public byte? serverId { get; set; }
    public bool orderByCraftPrice { get; set; } = false;
}