using AlternativeMkt.Db;

namespace AlternativeMkt.Main.Models;

public class SearchItemPrices
{
    public CraftItem Item { get; set; } = new();
    public List<CraftItemsPrice> Prices { get; set; } = new();
}