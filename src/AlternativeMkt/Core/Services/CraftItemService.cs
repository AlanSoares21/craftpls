using System.Linq.Expressions;
using AlternativeMkt.Admin.Models;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Services;

public class CraftItemService : ICraftItemService
{
    MktDbContext _db;
    public CraftItemService(MktDbContext db) {
        _db = db;
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