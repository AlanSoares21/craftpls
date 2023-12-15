
using AlternativeMkt.Admin.Models;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;
using AlternativeMkt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Main.Controllers;

public class CraftItemController: BaseController
{
    MktDbContext _db;
    ICraftItemService _craftItemService;
    public CraftItemController(
        MktDbContext db,
        ICraftItemService craftItemService) {
        _db = db;
        _craftItemService = craftItemService;
    }
    public async Task<IActionResult> Search(
        [FromQuery] ListItemsParams query) {
        ViewData["CraftItemQuery"] = query;
        return View(
            _craftItemService.SearchItems(query)
        );
    }
}