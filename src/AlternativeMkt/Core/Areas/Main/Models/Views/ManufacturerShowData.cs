using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;

namespace AlternativeMkt.Main.Models;

public class ManufacturerShowData
{
    public Manufacturer Manufacturer { get; set; } = null!;
    public StandardList<CraftItemsPrice> Prices { get; set; } = null!;
}