
using AlternativeMkt.Db;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Services;

public class PriceService : IPriceService
{
    MktDbContext _db;
    public PriceService(
        MktDbContext db) {
        _db = db;
    }
    public List<CraftItemsPrice> GetPricesForItem(int itemId)
    {
        return _db.CraftItemsPrices
            .Where(p => 
                p.ItemId == itemId
                && p.DeletedAt == null
                && !p.ResourcesChanged
            )
            .ToList();
    }

    public async Task ResourcesChanged(int itemId)
    {
        var prices = _db.CraftItemsPrices
            .Where(p => p.ItemId == itemId)
            .ToList();
        for (int i = 0; i < prices.Count; i++)
        {
            var price = prices[i];
            price.TotalPrice =  price.Price + 
                ResourcersTotalPriceFor(price.ItemId, price.ManufacturerId);
            price.ResourcesChanged = true;
        }
        await _db.SaveChangesAsync();
    }

    int ResourcersTotalPriceFor(int itemId, Guid manufacturerId)
    {
        int? resourcesPricesSum = _db.CraftItemsPrices
            .Where(p => 
                p.ManufacturerId == manufacturerId && 
                p.DeletedAt == null && 
                p.Item.ResourceFor.Where(r => r.ItemId == itemId).Count() > 0
            ).Sum(p => 
                p.TotalPrice * 
                p.Item.ResourceFor
                    .Where(r => r.ItemId == itemId)
                    .Single()
                    .Amount
            );
        if (resourcesPricesSum is null) {
            return 0;
        }
        return resourcesPricesSum.Value;
    }
}