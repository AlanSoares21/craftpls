
using AlternativeMkt.Admin.Models;
using AlternativeMkt.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Admin.Controllers;

public class ResourcesController : BaseAdminController
{
    MktDbContext _db; 
    ILogger<ResourcesController> _logger;
    public ResourcesController(
        MktDbContext db, 
        ILogger<ResourcesController> logger) {
        _db = db;
        _logger = logger;
    }
    [HttpGet("{itemId}")]
    // set item as resource
    public IActionResult New(int itemId, [FromQuery] SearchItemParams query)
    {
        var items = _db.CraftItems
            .Include(i => i.Asset)
            .Where(i => 
                (
                    i.Name != null && 
                    query.searchTerm != null && 
                    i.Name.ToLower().Contains(query.searchTerm)
                )
                &&
                (
                    (query.maxLevel == null || i.Level <= query.maxLevel) &&
                    (query.minLevel == null || i.Level >= query.minLevel)
                )
            )
            .Take(query.count)
            .Skip(query.startAt)
            .ToList();
        return View(items);
    }
    [HttpPost("{itemId}")]
    public IActionResult Add(
        int itemId,
        [Bind("resourceId,ammount")] AddResource resourceData) {
        return View();
    }
}