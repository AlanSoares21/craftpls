
using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;
using AlternativeMkt.Models;

namespace AlternativeMkt.Services;

public interface IPriceService {
    StandardList<CraftItemsPrice> Search(ListPricesParams query);
    Task AddPrice(AddItemPrice price);
    List<CraftItemsPrice> GetPricesForItem(int itemId);
    Task ResourcesChanged(int itemId);
    Task UpdatePrice(CraftItemsPrice price, UpdateItemPrice priceData);
    Task CheckResourcesChanged(CraftItemsPrice price);
}