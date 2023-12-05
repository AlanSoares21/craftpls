
using AlternativeMkt.Db;

namespace AlternativeMkt.Services;

public class CraftResourceService : ICraftResourceService
{
    MktDbContext _db;
    IPriceService _priceService;
    public CraftResourceService(
        MktDbContext db,
        IPriceService priceService) {
        _db = db;
        _priceService = priceService;
    }

    public async Task AddResource(CraftResource resource)
    {
        await _db.CraftResources.AddAsync(resource);
        await UpdatePricesForItem(resource.ItemId);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteResource(CraftResource resource)
    {
        _db.CraftResources.Remove(resource);
        await _db.SaveChangesAsync();
        await UpdatePricesForItem(resource.ItemId);
    }

    public async Task UpdateResource(CraftResource resourceData)
    {
        _db.CraftResources.Update(resourceData);
        await _db.SaveChangesAsync();
        await UpdatePricesForItem(resourceData.ItemId);
    }

    async Task UpdatePricesForItem(int itemId) {
        Queue<int> itemsToUpdate = new();
        itemsToUpdate.Enqueue(itemId);
        while (itemsToUpdate.Count != 0)
        {
            var currentItemId = itemsToUpdate.Dequeue();
            await _priceService.ResourcesChanged(currentItemId);
            var items = _db.CraftResources
                .Where(r => r.ResourceId == currentItemId)
                .ToList();
            for (int j = 0; j < items.Count; j++)
            {
                itemsToUpdate.Enqueue(items[j].ItemId);
            }
        }
    }
}