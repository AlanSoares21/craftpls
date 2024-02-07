using AlternativeMkt.Db;
using AlternativeMkt.Models;

namespace AlternativeMkt.Main.Models;

public class SearchItemPrices
{
    public CraftItem Item { get; set; } = new();
    public List<CraftItemsPrice> Prices { get; set; } = new();
    public ListPricesParams Query = new();
}