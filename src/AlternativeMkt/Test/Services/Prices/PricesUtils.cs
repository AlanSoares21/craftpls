
using AlternativeMkt.Db;

namespace AlternativeMkt.Tests.Services;

public class PricesUtils
{
    protected CraftResource GetCraftResource(
        int craftRersourceId, 
        CraftItem item, 
        CraftItem resourceItem) {    
        CraftResource craftResource = new() {
            Id = craftRersourceId,
            Amount = 1,
            ItemId = item.Id,
            Item = item,
            ResourceId = resourceItem.Id,
            Resource = resourceItem
        };
        resourceItem.ResourceFor.Add(craftResource);
        item.Resources.Add(craftResource);
        return craftResource;
    }

    protected CraftItemsPrice GetPrice(int price, int totalPrice, CraftItem item, Manufacturer manufacturer) {
        CraftItemsPrice craftPrice = new() {
            Id = Guid.NewGuid(),
            Manufacturer = manufacturer,
            ManufacturerId = manufacturer.Id,
            Item = item,
            ItemId = item.Id,
            Price = price,
            TotalPrice = totalPrice
        };
        item.Prices.Add(craftPrice);
        manufacturer.CraftItemsPrices.Add(craftPrice);
        return craftPrice;
    }
}