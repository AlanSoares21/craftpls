using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;
using AlternativeMkt.Models;

namespace AlternativeMkt.Main.Models;

public class ManufacturerShowData
{
    public Manufacturer Manufacturer { get; set; } = null!;
    public StandardList<CraftItemsPrice> Prices { get; set; } = null!;
    public ListPricesParams Query { get; set; } = null!;
}