using System.Linq.Expressions;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Api.Controllers;

[Authorize]
public class ItemsController: BaseApiController
{
    readonly MktDbContext _db;
    public ItemsController(MktDbContext db) {
        _db = db;
    }

    [HttpGet]
    public IActionResult ListItems([FromQuery] ListItemsParams query) {
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
        return Ok(list);
    }

    Expression<Func<CraftItem, bool>> FiltreItems(ListItemsParams query) {
        return i => 
        (query.name == null || i.Name != null && i.Name.StartsWith(query.name)) &&
        
        (query.level == null && query.maxLevel == null && query.minLevel == null || (
            query.level != null && query.level == i.Level
            || query.maxLevel != null && query.maxLevel >= i.Level
            || query.minLevel != null && query.minLevel <= i.Level
        ));
    }
}