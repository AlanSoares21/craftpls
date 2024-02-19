
using AlternativeMkt.Db;

namespace AlternativeMkt.Tests.Services;

public class PricesUtils
{
    protected CraftResource GetCraftResource(
        int craftRersourceId, 
        CraftItem item, 
        CraftItem resourceItem) {    
        return Utils.GetCraftResource(craftRersourceId, item, resourceItem);
    }

    protected CraftItemsPrice GetPrice(int price, int totalPrice, CraftItem item, Manufacturer manufacturer) {
        return Utils.GetPrice(price, totalPrice, item, manufacturer);
    }
}