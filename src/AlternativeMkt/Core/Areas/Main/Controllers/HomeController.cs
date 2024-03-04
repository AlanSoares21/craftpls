using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AlternativeMkt.Main.Models;
using AlternativeMkt.Services;
using AlternativeMkt.Models;
using AlternativeMkt.Api.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AlternativeMkt.Main.Controllers;

public class HomeController : BaseController
{
    IPriceService _priceService;
    IDistributedCache _cache;
    public HomeController(IPriceService priceService,
        IDistributedCache cache) {
        _priceService = priceService;
        _cache = cache;
    }
    public async Task<IActionResult> Index()
    {
        return View(await GetLatestPrices());
    }

    async Task<StandardList<Db.CraftItemsPrice>> GetLatestPrices() {
        string? latestPricesJson = await _cache.GetStringAsync("LatestPrices");
        StandardList<Db.CraftItemsPrice>? result;
        if (latestPricesJson is null) {
            result = _priceService.Search(new ListPricesParams() {
                orderByCreatedDate = true,
                onlyListItemsWithResources = true,
                count = 10
            });
            await _cache.SetStringAsync(
                "LatestPrices", 
                JsonSerializer.Serialize(result, new JsonSerializerOptions() {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                }),
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
