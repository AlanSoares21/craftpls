
using AlternativeMkt.Db;
using AlternativeMkt.Models;

namespace AlternativeMkt.Services;

public interface IPriceService {
    Task AddPrice(AddItemPrice price);
    List<CraftItemsPrice> GetPricesForItem(int itemId);
    Task ResourcesChanged(int itemId);
    Task CheckResourcesChanged(CraftItemsPrice price);
}