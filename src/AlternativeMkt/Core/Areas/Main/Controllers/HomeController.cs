using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AlternativeMkt.Main.Models;
using AlternativeMkt.Services;
using AlternativeMkt.Models;
using AlternativeMkt.Api.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlternativeMkt.Main.Controllers;

public class HomeController : BaseController
{
    IPriceService _priceService;
    IDistributedCache _cache;
    ILogger<HomeController> _logger;
    public HomeController(IPriceService priceService,
        IDistributedCache cache,
        ILogger<HomeController> logger
    ) {
        _priceService = priceService;
        _cache = cache;
        _logger = logger;
    }
    public async Task<IActionResult> Index()
    {
        return View(await GetLatestPrices());
    }

    async Task<StandardList<Db.CraftItemsPrice>> GetLatestPrices() {
        string culture = GetCulture();
        string cacheKey = "LatestPrices" + culture;
        string? latestPricesJson = await _cache.GetStringAsync(cacheKey);
        StandardList<Db.CraftItemsPrice>? result;
        if (latestPricesJson is null) {
            result = _priceService.Search(new ListPricesParams() {
                orderByCreatedDate = true,
                onlyListItemsWithResources = true,
                count = 10,
                culture = culture
            });
            string resultAsJson = JsonSerializer.Serialize(
                result, 
                new JsonSerializerOptions() {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                }
            );
            _logger.LogInformation("Storing prices in the cache for culture {culture} with the key {key} - length: {l}", 
                culture, 
                cacheKey,
                resultAsJson.Length
            );
            await _cache.SetStringAsync(
                cacheKey, 
                resultAsJson,
                new DistributedCacheEntryOptions() {
                    SlidingExpiration = TimeSpan.FromSeconds(60 * 3),
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60 * 5)
                }
            );
            return result;
        }
        result = JsonSerializer.Deserialize<StandardList<Db.CraftItemsPrice>>(latestPricesJson);
        if (result is null)
            throw new Exception("Fail on deserialize latests prices");
        return result;
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Tou()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
