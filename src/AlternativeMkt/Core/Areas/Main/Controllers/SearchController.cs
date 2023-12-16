using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Main.Controllers;

public class SearchController : BaseController {
    private readonly ILogger<SearchController> _logger;
    private MktDbContext _dbContext;

    public SearchController(
        MktDbContext dbContext,
        ILogger<SearchController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public IActionResult Manufacturers(int itemId) 
    {
        CraftItem item = _dbContext.CraftItems
            .Include("Asset")
            .Where(i => i.Id == itemId)
            .Single();
        var prices = _dbContext.CraftItemsPrices
            .Include(p => p.Manufacturer)
                .ThenInclude(m => m.Server)
            .Include(p => p.Manufacturer)
                .ThenInclude(m => m.User)
            .Where(p => 
                !p.Manufacturer.Hide 
                && p.ItemId == item.Id 
                && p.ResourcesChanged == false 
                && p.DeletedAt == null
            )
            .ToList();
        for (int i = 0; i < prices.Count; i++)
        {
            int serverId = prices[i].Manufacturer.ServerId;
            Guid userId = prices[i].Manufacturer.Userid;
            prices[i].Manufacturer.Server.GameAccounts = _dbContext.GameAccounts
                .Where(g => 
                    g.UserId == userId 
                    &&
                    g.ServerId == serverId 
                    && g.DeletedAt == null
                ).ToList();
        }
        return View(new SearchItemPrices() {
            Item = item,
            Prices = prices
        });
    }
}