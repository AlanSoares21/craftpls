using System.Linq.Expressions;
using System.Text.Json;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using AlternativeMkt.Models;
using AlternativeMkt.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Api.Controllers;


[Authorize(JwtBearerDefaults.AuthenticationScheme)]
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
        try {
            await _priceService.AddPrice(priceData);
            _logger.LogInformation("Item price added for user {u} - data: {data}",
                user.Id,
                dataAsJson
            );
            return Created("/", priceData);
        } catch(ServiceException ex) {
            _logger.LogError("Error on trying add new price - user: {user} - price data: {data} - error: {message}",
                user.Id,
                dataAsJson,
                ex.Message
            );
            return BadRequest(new ApiError(ex.Message));
        } catch(Exception ex) {
            _logger.LogCritical("Unhandled error on trying add new price - user: {user} - price data: {data} - error: {message} \n {stack}",
                user.Id,
                dataAsJson,
                ex.Message,
                ex.StackTrace
            );
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiError("Unhandled error. Try again later.")
            );
        }
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
        try {
            _logger.LogInformation("Updating price of {priceId} - old price: {old} - new price: {price} - user: {user}",
                itemPrice.Id,
                itemPrice.Price,
                data.price,
                user.Id
            );
            await _priceService.UpdatePrice(itemPrice, data);
            _logger.LogInformation("Updated item price {priceId} for user {u} - new price: {price}",
                itemPrice.Id,
                user.Id,
                itemPrice.Price
            );
            return NoContent();
        }
        catch (ServiceException ex) {
            _logger.LogError("Error when user {user} tried to update price {price} - new price: {prcie} - error: {error}",
                user.Id,
                itemPrice.Id,
                data.price,
                ex.Message
            );
            return BadRequest(new ApiError(ex.Message));
        }
        catch (Exception ex) {
            _logger.LogCritical("Unkowed error when user {user} tried to update price {price} - new price: {prcie} - error: {error} - {stack}",
                user.Id,
                itemPrice.Id,
                data.price,
                ex.Message,
                ex.StackTrace
            );
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new ApiError(ex.Message)
            );
        }
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