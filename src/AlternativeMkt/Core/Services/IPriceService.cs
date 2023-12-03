
using AlternativeMkt.Db;

namespace AlternativeMkt.Services;

public interface IPriceService {
    List<CraftItemsPrice> GetPricesForItem(int itemId);
    Task ResourcesChanged(int itemId);
}