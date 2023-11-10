using System.Text.Json;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Api.Controllers;


[Authorize]
public class PricesController : BaseApiController
{
    readonly MktDbContext _db;
    readonly ILogger<PricesController> _logger;
    readonly IAuthService _auth;
    public PricesController(
        MktDbContext db,
        ILogger<PricesController> logger,
        IAuthService auth
    ) {
        _db = db;
        _logger = logger;
        _auth = auth;
    }

    [Route("{manufacturerId}")]
    [HttpGet]
    public IActionResult List(
        Guid manufacturerId, 
        [FromQuery] StandardPaginationParams query
    ) {
        StandardList<CraftItemsPrice> list = new() {
            Start = query.start
        };
        list.Data = _db.CraftItemsPrices
            .Include(p => p.Item.Asset)
            .Where(p => 
                p.ManufacturerId == manufacturerId 
                && p.DeletedAt == null
            )
            .Skip(query.start)
            .Take(query.count)
            .ToList();
        list.Count = list.Data.Count;
        list.Total = _db.CraftItemsPrices.Where(p => 
            p.ManufacturerId == manufacturerId 
            && p.DeletedAt == null
        ).Count();
        return Ok(list);
    }
    
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddItemPrice price) {
        string dataAsJson = JsonSerializer.Serialize(price);
        var user = _auth.GetUser(User.Claims);
        _logger.LogInformation("Adding item price for user {Id} - data: {data}",
            user.Id,
            dataAsJson
        );
        // check if the manufacturer is owned by the user
        var manufacturer = await _db.Manufacturers
            .Include(m => m.CraftItemsPrices.Where(p => p.ItemId == price.itemId))
            .Where(m => m.Id == price.manufacturerId && m.Userid == user.Id)
            .SingleOrDefaultAsync();
        if (manufacturer is null) {
            _logger.LogError("Manufacturer {m} not found - user: {u}", 
                price.manufacturerId,
                user.Id
            );
            return NotFound(new ApiError($"Manufacturer {price.manufacturerId} not found"));
        }
        // check if already exists a price for this item
        if (manufacturer.CraftItemsPrices.Count > 0) {
            _logger.LogError("Manufacturer {m} already set a price for item {item}", 
                price.manufacturerId,
                price.itemId
            );
            return BadRequest(new ApiError($"You alredy set a price for this item {manufacturer.CraftItemsPrices.ElementAt(0).ItemId}"));
        }
        CraftItemsPrice itemsPrice = new() {
            ItemId = price.itemId,
            ManufacturerId = price.manufacturerId,
            Price = price.price
        };
        await _db.CraftItemsPrices.AddAsync(itemsPrice);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Item price added for user {u} - data: {data}",
            user.Id,
            dataAsJson
        );
        return Created("/", price);
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
        var itemPrice = await _db.CraftItemsPrices
            .Include(p => p.Manufacturer)
            .Where(p => p.Id == priceId)
            .SingleOrDefaultAsync();
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

}