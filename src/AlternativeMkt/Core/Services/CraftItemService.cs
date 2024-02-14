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
            i.Name != null && 
            EF.Functions.ILike(
                i.Name,
                $"%{query.name}%"
            )
        ) 
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