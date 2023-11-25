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
        
        (
            query.level == null && 
                (query.maxLevel == null || query.maxLevel != null && query.maxLevel >= i.Level) 
                && (query.minLevel == null || query.minLevel != null && query.minLevel <= i.Level)
            ||
            query.level != null && query.level == i.Level 
        );
    }

    [HttpGet("{itemId}/resources")]
    public IActionResult ListResources(int itemId) {
        StandardList<CraftResource> response = new();
        response.Start = 0;
        response.Data = _db.CraftResources
            .Include(r => r.Resource)
                .ThenInclude(i => i.Asset)
            .Where(r => r.ItemId == itemId)
            .ToList();
        response.Count = response.Data.Count;
        response.Total = response.Data.Count;
        return Ok(response);
    }
}