
using System.Linq.Expressions;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;
using AlternativeMkt.Models;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Services;

public class PriceService : IPriceService
{
    MktDbContext _db;
    ILogger<PriceService> _logger;
    public PriceService(
        MktDbContext db,
        ILogger<PriceService> logger) {
        _db = db;
        _logger = logger;
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
            FixTotalPrice(price);
            price.ResourcesChanged = true;
        }
        await _db.SaveChangesAsync();
    }
    void FixTotalPrice(CraftItemsPrice price)
    {
        price.TotalPrice =  price.Price + 
                ResourcesTotalPriceFor(price.ItemId, price.ManufacturerId);
    }

    int ResourcesTotalPriceFor(int itemId, Guid manufacturerId)
    {
        int? resourcesPricesSum = _db.CraftItemsPrices
            .Include(p => p.Item)
                .ThenInclude(i => i.ResourceFor.Where(r => r.ItemId == itemId))
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
    

    public async Task AddPrice(AddItemPrice priceData)
    {
        var item = _db.CraftItems
            .Include(i => i.Prices
                .Where(p => p.ManufacturerId == priceData.manufacturerId
                    && p.DeletedAt == null
                )
            )
            .Include(i => i.Resources)
                .ThenInclude(r => r.Resource)
                    .ThenInclude(ri => ri.Prices
                        .Where(p => 
                            p.ManufacturerId == priceData.manufacturerId
                            && p.DeletedAt == null
                        )
                )
            .Where(i => i.Id == priceData.itemId)
            .SingleOrDefault();
        if (item is null)
            throw new ServiceException($"Item {priceData.itemId} not found");
        if (item.Prices.Count > 0)
            throw new ServiceException($"You already has added a price for this item");
        AllResourcesHavePrices(item);
        CraftItemsPrice itemPrice = new() {
            ItemId = item.Id,
            Item = item,
            ManufacturerId = priceData.manufacturerId,
            Price = priceData.price,
            TotalPrice = ResourcesTotalPrice(item.Resources) + priceData.price
        };

        await _db.CraftItemsPrices.AddAsync(itemPrice);
        await _db.SaveChangesAsync();
        await UpdateDependentsPrices(itemPrice.ItemId, itemPrice.ManufacturerId);
    }

    void AllResourcesHavePrices(CraftItem item) {
        var resources = item.Resources.Select(r => r.Resource);
        var missingResource = resources
            .FirstOrDefault(i => i.Prices.Count == 0);
        if (missingResource is not null)
            throw new ServiceException($"The price for the resource {missingResource.Name}({missingResource.Id}) is missing.");
    }

    int ResourcesTotalPrice(IEnumerable<CraftResource> craftResources) {
        int? result = craftResources.Sum(r => 
            r.Amount * r.Resource.Prices.ElementAt(0).TotalPrice
        );
        if (result is null)
            return 0;
        return result.Value;
    }

    public async Task UpdatePrice(
        CraftItemsPrice price, 
        UpdateItemPrice priceData)
    {
        /*
            Obs: you probably dont need to use the FixTotalPrice function,
            you could get the difference in the total price and 
            apply this difference in the craft items prices that rely
            on the updated item price
            (you probably could do the same where the FixTotalPrice is originally used)
        */
        price.TotalPrice = price.TotalPrice - price.Price + priceData.price;
        price.Price = priceData.price;
        _db.CraftItemsPrices.Update(price);
        await _db.SaveChangesAsync();
        await UpdateDependentsPrices(price.ItemId, price.ManufacturerId);
    }

    async Task UpdateDependentsPrices(int itemId, Guid manufacturerId) {
        var resources = GetCraftResourcesByItemResourceId(itemId, manufacturerId)
            .ToList();
        _logger.LogInformation("Updating prices for the resource {itemId} - manufacturer: {manufacturer} - Count: {count}",
            itemId,
            manufacturerId,
            resources.Count
        );
        for (int i = 0; i < resources.Count; i++) {
            if (resources[i].Item.Prices.Count > 0) {
                var resourcePrice = resources[i].Item
                    .Prices.ElementAt(0);
                FixTotalPrice(resourcePrice);
                _db.CraftItemsPrices.Update(resourcePrice);
                var itemResources = GetCraftResourcesByItemResourceId(
                    resourcePrice.ItemId,
                    resourcePrice.ManufacturerId
                );
                resources.AddRange(itemResources);
                await _db.SaveChangesAsync();
            }
            else 
                _logger.LogInformation("Manufacturer {manufacturer} dont have a price for {itemId}",
                    manufacturerId,
                    resources[i].ItemId
                );
        }
        _logger.LogInformation("While updating prices from manufacturer {manufacturerId} for the reosurce {itemId}, {count} items total prices was updated",
            manufacturerId,
            itemId,
            resources.Count
        );
    }

    IQueryable<CraftResource> GetCraftResourcesByItemResourceId(
        int itemResourceId,
        Guid manufacturerId) {
        return _db.CraftResources
            .Include(r => r.Item)
                .ThenInclude(i => i.Prices
                    .Where(p => 
                        p.ManufacturerId == manufacturerId
                        && p.DeletedAt == null
                    )
                )
            .Where(r => r.ResourceId == itemResourceId);
    }

    public StandardList<CraftItemsPrice> Search(ListPricesParams query)
    {
        StandardList<CraftItemsPrice> list = new() {
            Start = query.start
        };
        var sqlQuery = _db.CraftItemsPrices
            .Include(p => p.Item)
                .ThenInclude(i => i.Asset)
            .Include(p => p.Item.DataByCulture
                .Where(d => d.Culture == query.culture))
            .Include(p => p.Item.Attributes)
                .ThenInclude(a => a.Attribute)
            .Include(p => p.Manufacturer)
                .ThenInclude(m => m.Server)
            .Include(p => p.Manufacturer)
                .ThenInclude(m => m.User)
            .Where(FilterPrices(query));
        if (query.orderByCreatedDate)
            sqlQuery = sqlQuery.OrderByDescending(p => p.CreatedAt);
        else if (query.orderByCraftPrice)
            sqlQuery = sqlQuery.OrderBy(p => p.Price);
        else
            sqlQuery = sqlQuery.OrderBy(p => p.TotalPrice);
        list.Data = sqlQuery
            .Skip(query.start)
            .Take(query.count)
            .ToList();
        list.Count = list.Data.Count;
        list.Total = _db.CraftItemsPrices
            .Where(FilterPrices(query))
            .Count();
        GetManufacturersAccountsNames(list.Data);
        return list;
    }

    Expression<Func<CraftItemsPrice, bool>> FilterPrices(
        ListPricesParams query) {
        return p => 
            p.DeletedAt == null
            && !p.Manufacturer.Hide
            && (query.manufacturerId == null || p.ManufacturerId == query.manufacturerId)
            && (query.serverId == null || p.Manufacturer.ServerId == query.serverId)
            && (query.resourcesChanged == null || p.ResourcesChanged == query.resourcesChanged)
            && (
                query.itemId == null 
                || p.ItemId == query.itemId
            ) 
            && (
                query.resourcesOf == null
                || p.Item.ResourceFor.Where(r => r.ItemId == query.resourcesOf).Count() > 0 
            )
            && (!query.onlyListItemsWithResources || p.Item.Resources.Count() > 0)
            && (query.itemName == null ||
                (query.culture != null && query.culture != "en" && 
                    p.Item.DataByCulture.Count > 0
                    &&
                    p.Item.DataByCulture.First().Culture == query.culture
                    &&
                    EF.Functions.ILike(
                        p.Item.DataByCulture.First().Name,
                        $"%{query.itemName}%"
                    )
                )
                ||
                ((query.culture == null || query.culture == "en") &&
                    p.Item.Name != null 
                    && 
                    EF.Functions.ILike(
                        p.Item.Name,
                        $"%{query.itemName}%"
                    )
                )
            ) 
            && (query.itemMaxLevel == null || p.Item.Level <= query.itemMaxLevel)
            && (query.itemMinLevel == null || p.Item.Level >= query.itemMinLevel)
            && (query.itemCategory == null || query.itemCategory == -1 ||
                p.Item.CategoryId != null && p.Item.CategoryId == query.itemCategory
            );
    }

    void GetManufacturersAccountsNames(List<CraftItemsPrice> prices) {
        for (int i = 0; i < prices.Count; i++)
        {
            int serverId = prices[i].Manufacturer.ServerId;
            Guid userId = prices[i].Manufacturer.Userid;
            prices[i].Manufacturer.Server.GameAccounts = _db.GameAccounts
                .Where(g => 
                    g.UserId == userId 
                    &&
                    g.ServerId == serverId 
                    && g.DeletedAt == null
                ).ToList();
        }
    }
}