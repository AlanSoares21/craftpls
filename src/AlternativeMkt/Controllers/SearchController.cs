using AlternativeMkt.Db;
using AlternativeMkt.Models;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Controllers;

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
    public IActionResult Index(string searchTerm) 
    {
        _logger.LogInformation("Search index run");
        ViewData["CraftItemSearched"] = searchTerm;
        return View(_dbContext.CraftItemsPrices
            .Where(p => p.Item.Name != null && p.Item.Name.Contains(searchTerm))
        );
    }
}