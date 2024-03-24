using System.Linq.Expressions;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Api.Controllers;

[Authorize(JwtBearerDefaults.AuthenticationScheme)]
public class AssetsController: BaseApiController {
    MktDbContext _db;
    ILogger<AssetsController> _logger;
    public AssetsController(MktDbContext db, 
        ILogger<AssetsController> logger) {
        _db = db;
        _logger = logger;
    }
    [HttpGet]
    public IActionResult List(
        [FromQuery] ListAssetsParams query) 
    {
        if (query.count > 10 || query.count < 1)
            query.count = 10;
        StandardList<Asset> response = new() {
            Start = query.start,
        };
        response.Data = _db.Assets.Where(FilterAssets(query))
            .Skip(query.start)
            .Take(query.count)
            .OrderByDescending(a => a.Id)
            .ToList();
        response.Count = response.Data.Count;
        response.Total = _db.Assets.Where(FilterAssets(query)).Count();
        return Ok(response);
    }

    public Expression<Func<Asset, bool>> FilterAssets(
        ListAssetsParams query) {
            return a => 
                (
                    query.endpoint == null ||
                    EF.Functions.ILike(a.Endpoint, query.endpoint)
                )
                &&
                (
                    query.itemName == null ||
                    a.CraftItems.Any(c => 
                        c.Name != null &&
                        EF.Functions.ILike(c.Name, $"%{query.itemName}%")
                    )
                )
                &&
                (
                    query.unusedAssets == null 
                    ||
                    query.unusedAssets == true && a.CraftItems.Count == 0 
                    ||
                    query.unusedAssets != null 
                    &&  query.unusedAssets == false 
                    && a.CraftItems.Count > 0
                );
        }

}