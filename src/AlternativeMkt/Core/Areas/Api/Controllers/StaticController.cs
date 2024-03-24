using System.Net;
using System.Text;
using System.Text.Json;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace AlternativeMkt.Api.Controllers;

public class StaticController: BaseApiController {

    IDistributedCache _cache;
    MktDbContext _db;
    ILogger<StaticController> _logger;
    public StaticController(IDistributedCache cache, MktDbContext db, ILogger<StaticController> logger) {
        _cache = cache;
        _db = db;
        _logger = logger;
    }
    [HttpGet("data")]
    public async Task<IActionResult> GetData() {
        try {
            return Ok(new StaticData() {
                Categories = await GetCategories(),
                Attributes = await GetAttributes()
            });
        } catch (Exception ex) {
            _logger.LogCritical("Unknowed error: {message} \n {stack}", ex.Message, ex.StackTrace);
            return StatusCode((int)HttpStatusCode.InternalServerError, new ApiError("Error retrive static data"));
        }
    }

    async Task<List<CraftCategory>> GetCategories() {
        var data = await _cache.GetStringAsync("CraftCategories");
        List<CraftCategory> categories;
        if (string.IsNullOrEmpty(data)) {
            categories = _db.CraftCategories.ToList();
            await _cache.SetStringAsync(
                "CraftCategories", 
                JsonSerializer.Serialize(categories), 
                options: new DistributedCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = new TimeSpan(0, 30, 0),
                    SlidingExpiration = new TimeSpan(0, 5, 0)
                }
            );
        } else {
            var deserializedData = JsonSerializer.Deserialize<List<CraftCategory>>(data);
            if (deserializedData is null)
                categories = new();
            else
                categories = deserializedData;
        }
        return categories;
    }

    async Task<List<Db.Attribute>> GetAttributes() {
        var data = await _cache.GetStringAsync("ItemAttributes");
        List<Db.Attribute> attributes;
        if (string.IsNullOrEmpty(data)) {
            attributes = _db.Attributes.ToList();
            await _cache.SetStringAsync(
                "ItemAttributes", 
                JsonSerializer.Serialize(attributes), 
                options: new DistributedCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = new TimeSpan(0, 30, 0),
                    SlidingExpiration = new TimeSpan(0, 5, 0)
                }
            );
        } else {
            var deserializedData = JsonSerializer
                .Deserialize<List<Db.Attribute>>(data);
            if (deserializedData is null)
                attributes = new();
            else
                attributes = deserializedData;
        }
        return attributes;
    }
    
}