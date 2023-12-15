using System.Linq.Expressions;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;
using AlternativeMkt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Api.Controllers;

[Authorize]
public class ItemsController: BaseApiController
{
    MktDbContext _db;
    ICraftItemService _craftItemService;
    public ItemsController(
        MktDbContext db,
        ICraftItemService craftItemService) {
        _db = db;
        _craftItemService = craftItemService;
    }

    [HttpGet]
    public IActionResult ListItems([FromQuery] ListItemsParams query) {
        return Ok(_craftItemService.SearchItems(query));
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