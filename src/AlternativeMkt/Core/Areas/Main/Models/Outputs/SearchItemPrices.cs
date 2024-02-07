using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;
using AlternativeMkt.Models;

namespace AlternativeMkt.Main.Models;

public class SearchItemPrices
{
    public CraftItem Item { get; set; } = new();
    public StandardList<CraftItemsPrice> Prices { get; set; } = new();
    public ListPricesParams Query = new();
}