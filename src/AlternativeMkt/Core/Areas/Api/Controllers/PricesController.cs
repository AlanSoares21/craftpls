using System.Linq.Expressions;
using System.Text.Json;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using AlternativeMkt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Api.Controllers;


[Authorize]
public class PricesController : BaseApiController
{
    MktDbContext _db;
    ILogger<PricesController> _logger;
    IAuthService _auth;
    IPriceService _priceService;
    public PricesController(
        MktDbContext db,
        ILogger<PricesController> logger,
        IAuthService auth,
        IPriceService priceService
    ) {
        _db = db;
        _logger = logger;
        _auth = auth;
        _priceService = priceService;
    }

    [Route("{manufacturerId}")]
    [HttpGet]
    public IActionResult List(
        Guid manufacturerId, 
        [FromQuery] ListPricesParams query
    ) {
        StandardList<CraftItemsPrice> list = new() {
            Start = query.start
        };
        list.Data = _db.CraftItemsPrices
            .Include(p => p.Item.Asset)
            .Where(FilterPrices(query, manufacturerId))
            .Skip(query.start)
            .Take(query.count)
            .ToList();
        list.Count = list.Data.Count;
        list.Total = _db.CraftItemsPrices
            .Where(FilterPrices(query, manufacturerId))
            .Count();
        return Ok(list);
    }

    Expression<Func<CraftItemsPrice, bool>> FilterPrices(
        ListPricesParams query, Guid manufacturerId) {
        return p => 
            p.ManufacturerId == manufacturerId 
            && p.DeletedAt == null
            && (
                query.itemId == null 
                || p.ItemId == query.itemId
            ) && (
                query.resourcesOf == null
                || p.Item.ResourceFor.Where(r => r.ItemId == query.resourcesOf).Count() > 0 
            );
    }
    
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddItemPrice priceData) {
        string dataAsJson = JsonSerializer.Serialize(priceData);
        var user = _auth.GetUser(User.Claims);
        _logger.LogInformation("Adding item price for user {Id} - data: {data}",
            user.Id,
            dataAsJson
        );
        if (priceData.price < 1) {
            _logger.LogInformation("User {id} tried add price with value {v}", 
                user.Id,
                priceData.price
            );
            return BadRequest(new ApiError("Price must be greater than zero"));
        }
        // check if the manufacturer is owned by the user
        var manufacturer = await _db.Manufacturers
            .Include(m => m.CraftItemsPrices.Where(p => p.ItemId == priceData.itemId))
            .Where(m => m.Id == priceData.manufacturerId && m.Userid == user.Id)
            .SingleOrDefaultAsync();
        if (manufacturer is null) {
            _logger.LogError("Manufacturer {m} not found - user: {u}", 
                priceData.manufacturerId,
                user.Id
            );
            return NotFound(new ApiError($"Manufacturer {priceData.manufacturerId} not found"));
        }
        // check if already exists a price for this item
        if (manufacturer.CraftItemsPrices.Count > 0 && 
/*
    OBS: when mocking entity framework the include method dont work as at runtime, 
    so its necessary check the first element of CraftItemsPrices
*/
#if DEBUG
            manufacturer.CraftItemsPrices[0].ItemId == priceData.itemId
#endif
        ) {
            _logger.LogError("Manufacturer {m} already set a price for item {item}", 
                priceData.manufacturerId,
                priceData.itemId
            );
            return BadRequest(new ApiError($"You have alredy set a price for this item {manufacturer.CraftItemsPrices.ElementAt(0).ItemId}"));
        }
        CraftItem? item = _db.CraftItems
            .Include(i => i.Resources)
            .Where(i => i.Id == priceData.itemId).SingleOrDefault();
        if (item is null) {
            _logger.LogInformation("User {user} tried to add price to item {id}, but could not found the item in the db", 
                user.Id,
                priceData.itemId
            );
            return BadRequest(new ApiError($"Item {priceData.itemId} not found in db"));
        }
        int totalPrice = priceData.price;
        if (item.Resources.Count > 0) {
            int? resourcePrices = _db.CraftItemsPrices
                .Include(p => p.Item)
                    .ThenInclude(i => i.ResourceFor
                        .Where(r => r.ItemId == item.Id)
                    )
                .Where(p => p.ManufacturerId == manufacturer.Id && p.Item.ResourceFor
                    .Where(r => r.ItemId == item.Id).Count() > 0
                ).Sum(p => p.TotalPrice * p.Item.ResourceFor.Where(r => r.ItemId == item.Id).Sum(r => r.Amount));
            if (resourcePrices is not null && resourcePrices.Value > 0)
                totalPrice += resourcePrices.Value;
            else {
                _logger.LogError("User {user} tried to set price to item {item}, but have not added prices to item resources - data: {data}",
                    user.Id,
                    item.Id,
                    dataAsJson
                );
                return BadRequest(new ApiError("Add prices to item resources before add a price to it"));
            }
        }
        CraftItemsPrice itemsPrice = new() {
            ItemId = priceData.itemId,
            ManufacturerId = priceData.manufacturerId,
            Price = priceData.price,
            TotalPrice = totalPrice
        };
        await _db.CraftItemsPrices.AddAsync(itemsPrice);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Item price added for user {u} - data: {data}",
            user.Id,
            dataAsJson
        );
        return Created("/", priceData);
    }

    [Route("{priceId}")]
    [HttpPut]
    public async Task<IActionResult> Update(
        Guid priceId, 
        [FromBody] UpdateItemPrice data
    ) {
        var user = _auth.GetUser(User.Claims);
        _logger.LogInformation("Updating price {id} for user {Id} - price: {p}",
            priceId,
            user.Id,
            data.price
        );
        var itemPrice = await SearchPrice(priceId);
        if (itemPrice is null) {
            _logger.LogInformation("item price with id {id} not found",
                priceId
            );
            return NotFound(new ApiError("Craft item price not found"));
        }
        if (itemPrice.Manufacturer.Userid != user.Id) {
            _logger.LogInformation("the user {user} tryed update item price {price} but the owner of the manufacturer is {ownerId}",
                user.Id,
                priceId,
                itemPrice.Manufacturer.Id
            );
            return Unauthorized(new ApiError("You dont own this craft item price"));
        }
        _logger.LogInformation("Updating price of {priceId} - old price: {old} - new price: {price} - user: {user}",
            itemPrice.Id,
            itemPrice.Price,
            data.price,
            user.Id
        );
        itemPrice.Price = data.price;
        DateTime updatedAt = new(DateTime.UtcNow.Ticks);
        var result = await _db.CraftItemsPrices
            .Where(p => p.Id == itemPrice.Id)
            .ExecuteUpdateAsync(st => st
                .SetProperty(p => p.UpdatedAt, updatedAt)
                .SetProperty(p => p.Price, data.price)
            );
        if (result == 0) {
            _logger.LogError("Fail to update item price {priceid} - price: {price} - user: {user}",
                itemPrice.Id,
                itemPrice.Price,
                user.Id
            );
            return BadRequest(new ApiError("Fail on update item price. No changes made"));
        }
        _logger.LogInformation("Updated item price {priceId} for user {u} - new price: {price}",
            itemPrice.Id,
            user.Id,
            itemPrice.Price
        );
        return NoContent();
    }

    [Route("{priceId}/checkResources")]
    [HttpPut]
    public async Task<IActionResult> CheckResources(Guid priceId) {
        var user = _auth.GetUser(User.Claims);
        _logger.LogInformation("Checking resources of price {id} for user {Id}",
            priceId,
            user.Id
        );
        var itemPrice = await SearchPrice(priceId);
        string logPrefix = $"User {user.Id} tried to check resources of price {priceId}";
        if (itemPrice is null) { 
            _logger.LogError("{prefix} but the price has not been found", logPrefix);
            return BadRequest(new ApiError($"Price {priceId} not found"));
        }
        if (itemPrice.Manufacturer.Userid != user.Id) {
            _logger.LogError("{prefix} but the manufacturer {id} is owned by other user", 
                logPrefix,
                itemPrice.ManufacturerId
            );
            return Unauthorized(new ApiError($"You can only check resources for your own items prices"));
        }
        try {
            _logger.LogInformation("Checking resources of price {price} for user {user}",
                itemPrice.Id,
                user.Id
            );
            await _priceService.CheckResourcesChanged(itemPrice);
            _logger.LogInformation("Resources checked for price {price} by user {user}",
                itemPrice.Id,
                user.Id
            );
            return NoContent();
        } catch(ServiceException ex) {
            _logger.LogError("{prefix} but an service exception ocurred. Message: {message}", logPrefix, ex.Message);
            return BadRequest(new ApiError(ex.Message));
        } catch (Exception ex) {
            _logger.LogError("{prefix} but a exception ocurred. Message: {message} \n Stack: {stack}", 
                logPrefix, 
                ex.Message,
                ex.StackTrace
            );
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new ApiError("Unexpected error ocurred. Try again later.")
            );
        }
    }

    Task<CraftItemsPrice?> SearchPrice(Guid priceId) {
        return _db.CraftItemsPrices
            .Include(p => p.Manufacturer)
            .Where(p => p.Id == priceId && p.DeletedAt == null)
            .SingleOrDefaultAsync();
    }
}