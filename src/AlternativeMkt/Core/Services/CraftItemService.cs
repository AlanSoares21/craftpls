using System.Linq.Expressions;
using AlternativeMkt.Admin.Models;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Services;

public class CraftItemService : ICraftItemService
{
    MktDbContext _db;
    ILogger<CraftItemService> _logger;
    public CraftItemService(MktDbContext db, ILogger<CraftItemService> logger) {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> Add(CraftItem item)
    {
        await _db.CraftItems.AddAsync(item);
        int rowsAffected = await _db.SaveChangesAsync();
        return rowsAffected == 
            1 + item.Attributes.Count + item.DataByCulture.Count;
    }

    public async Task<bool> Delete(int itemId)
    {
        var item = _db.CraftItems
            .Include(i => i.ResourceFor)
            .SingleOrDefault(i => i.Id == itemId);
        if (item is null)
            throw new ServiceException($"Item {itemId} not found");
        if (item.ResourceFor.Count > 0) {
            _logger.LogError("Can not delete item {id} because it is resource for {amount} items", item.Id, item.ResourceFor.Count);
            throw new ServiceException($"Item {itemId} is resource for other items.");
        }
        _logger.LogInformation("Deleting item {name}({id})", item.Name, item.Id);
        _db.CraftItems.Remove(item);
        int rowsAffected = await _db.SaveChangesAsync();
        return rowsAffected == 1;
    }

    public StandardList<CraftItem> SearchItems(ListItemsParams query)
    {
        StandardList<CraftItem> list = new() {
            Start = query.start
        };
        list.Data = _db.CraftItems
            .Include(i => i.Asset)
            .Include(i => i.Attributes)
                .ThenInclude(a => a.Attribute)
            .Include(i => i.DataByCulture
                .Where(d => d.Culture == query.culture))
            .Where(FiltreItems(query))
            .OrderBy(i => i.Id)
            .Skip(query.start)
            .Take(query.count)
            .ToList();
        list.Count = list.Data.Count;
        list.Total = _db.CraftItems.Where(FiltreItems(query))
            .Count();
        return list;
    }

    Expression<Func<CraftItem, bool>> FiltreItems(ListItemsParams query) {
        return i => 
        (
            query.name == null || 
            (
                query.culture != null && query.culture != "en" && 
                i.DataByCulture.Count > 0
                &&
                i.DataByCulture.First().Culture == query.culture
                && 
                EF.Functions.ILike(
                    i.DataByCulture.First().Name,
                    $"%{query.name}%"
                )
            )
            ||
            ((query.culture == null || query.culture == "en") && (
                i.Name != null && 
                EF.Functions.ILike(
                    i.Name,
                    $"%{query.name}%"
                )
            ))
        )  
        && (!query.onlyListItemsWithResources || i.Resources.Count > 0)
        && (query.categoryId == null || i.CategoryId == query.categoryId)
        &&
        (
            query.level == null && 
                (query.maxLevel == null || query.maxLevel != null && query.maxLevel >= i.Level) 
                && (query.minLevel == null || query.minLevel != null && query.minLevel <= i.Level)
            ||
            query.level != null && query.level == i.Level 
        );
    }
}