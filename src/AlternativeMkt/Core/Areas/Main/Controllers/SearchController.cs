using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using AlternativeMkt.Models;
using AlternativeMkt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Main.Controllers;

public class SearchController : BaseController {
    private readonly ILogger<SearchController> _logger;
    private MktDbContext _dbContext;
    IPriceService _priceService;

    public SearchController(
        MktDbContext dbContext,
        ILogger<SearchController> logger,
        IPriceService priceService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _priceService = priceService;
    }
    public IActionResult Manufacturers(int itemId, int serverId = -1, string? orderByCraftPrice = null) 
    {
        CraftItem item = _dbContext.CraftItems
            .Include("Asset")
            .Where(i => i.Id == itemId)
            .Single();
        ListPricesParams query = new();
        query.itemId = item.Id;
        query.resourcesChanged = false;
        query.orderByCraftPrice = orderByCraftPrice == "on";
        if (serverId > 0 && serverId > byte.MinValue && serverId < byte.MaxValue)
            query.serverId = (byte)serverId;
        var result = _priceService.Search(query);
        return View(new SearchItemPrices() {
            Item = item,
            Prices = result,
            Query = query
        });
    }
}