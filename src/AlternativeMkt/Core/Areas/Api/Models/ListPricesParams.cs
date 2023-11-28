namespace AlternativeMkt.Api.Models;

public class ListPricesParams: StandardPaginationParams
{
    public int? itemId { get; set; }
    public int? resourcesOf { get; set; }
}