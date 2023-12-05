
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

    public async Task CheckResourcesChanged(CraftItemsPrice price)
    {
        var resources = GetAllResources(price.ItemId);
        var prices = GetPricesForResources(price.ManufacturerId, resources);
        SomeResourceNeedAPrice(resources, prices);
        SomePriceNeedsToBeChecked(prices);
        price.ResourcesChanged = false;
        _db.CraftItemsPrices.Update(price);
        await _db.SaveChangesAsync();
    }

    List<CraftResource> GetAllResources(int firstItemId) {
        List<CraftResource> resources = new();
        Queue<int> itemsToGetResources = new();
        itemsToGetResources.Enqueue(firstItemId);
        while (itemsToGetResources.Count > 0) {
            int itemId = itemsToGetResources.Dequeue();
            var itemResources = _db.CraftResources
                .Include(r => r.Resource)
                .Where(r => r.ItemId == itemId)
                .ToList();
            foreach (var resource in itemResources)
            {
                resources.Add(resource);
                itemsToGetResources.Enqueue(resource.ResourceId);
            }
        }
        return resources;
    }

    List<CraftItemsPrice> GetPricesForResources(
        Guid manufacturerId, 
        List<CraftResource> resources) {
        var ids = resources.Select(r => r.ResourceId);
        return _db.CraftItemsPrices
            .Include(p => p.Item)
            .Where(p => 
                p.ManufacturerId == manufacturerId &&
                p.DeletedAt == null &&
                ids.Contains(p.ItemId)
            )
            .ToList();
    }

    void SomePriceNeedsToBeChecked(List<CraftItemsPrice> prices) {
        CraftItemsPrice? price = prices.Find(p => p.ResourcesChanged);
        if (price is not null) {
            throw new ServiceException($"The price for item {price.Item.Name}({price.ItemId}) should be checked first");
        }
    }

    void SomeResourceNeedAPrice(
        List<CraftResource> resources, 
        List<CraftItemsPrice> prices) {
        if (prices.Count != resources.Count) {
            CraftResource? craftResource = resources.Find(r => 
                !prices.Exists(p => r.ResourceId == p.ItemId)
            );
            if (craftResource is null)
                throw new ServiceException("The number of prices does not match the number of resources required.");
            throw new ServiceException($"Missing price for item {craftResource.Resource.Name}({craftResource.ResourceId})");
        }
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