
using AlternativeMkt.Admin.Models;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;
using AlternativeMkt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Main.Controllers;

public class CraftItemController: BaseController
{
    ICraftItemService _craftItemService;
    public CraftItemController(
        ICraftItemService craftItemService) {
        _craftItemService = craftItemService;
    }
    public IActionResult Search(
        [FromQuery] ListItemsParams query) {
        if (query.categoryId < 1)
            query.categoryId = null;
        ViewData["CraftItemQuery"] = query;
        return View(
            _craftItemService.SearchItems(query)
        );
    }
}