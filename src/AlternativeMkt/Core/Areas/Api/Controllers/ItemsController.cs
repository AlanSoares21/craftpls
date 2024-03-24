using System.Net;
using System.Text.Json;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using AlternativeMkt.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Api.Controllers;

[Authorize(JwtBearerDefaults.AuthenticationScheme)]
public class ItemsController: BaseApiController
{
    MktDbContext _db;
    ICraftItemService _craftItemService;
    ILogger<ItemsController> _logger;
    IAuthService _auth;
    public ItemsController(
        MktDbContext db,
        ICraftItemService craftItemService,
        ILogger<ItemsController> logger,
        IAuthService auth) {
        _db = db;
        _craftItemService = craftItemService;
        _logger = logger;
        _auth = auth;
    }

    [HttpGet]
    public IActionResult ListItems([FromQuery] ListItemsParams query) {
        try {
            var result = _craftItemService.SearchItems(query);
            return Ok(result);
        } catch(Exception ex) {
            _logger.LogCritical("Unknown error on listing items. Error: {message} \n {stack}",
                ex.Message,
                ex.StackTrace
            );
            return StatusCode(
                (int)HttpStatusCode.InternalServerError, 
                new ApiError("Unknown Error on listing items. try again later")
            );
        }
    }

    [HttpGet("{itemId}")]
    public IActionResult Get(int itemId) {
        CraftItem? response = _db.CraftItems
            .Include(i => i.Resources)
                .ThenInclude(r => r.Resource)
                    .ThenInclude(i => i.Asset)
            .Include(i => i.Asset)
            .Include(i => i.Attributes)
                .ThenInclude(a => a.Attribute)
            .Where(i => i.Id == itemId)
            .SingleOrDefault();
        if (response is null)
            return NotFound(new ApiError($"Item {itemId} not found"));
        return Ok(response);
    }

    [HttpGet("{itemId}/resources")]
    public IActionResult ListResources(int itemId) {
        StandardList<CraftResource> response = new();
        response.Start = 0;
        response.Data = _db.CraftResources
            .Include(r => r.Resource)
                .ThenInclude(i => i.Asset)
            .Where(r => r.ItemId == itemId)
            .ToList();
        response.Count = response.Data.Count;
        response.Total = response.Data.Count;
        return Ok(response);
    }
    
    [Authorize(JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [HttpDelete("{itemId}")]
    public async Task<IActionResult> Delete(int itemId) {
        try {
            User user = _auth.GetUser(User.Claims);
            _logger.LogInformation("User {user}({userId}) try delete item {id}", 
                user.Email,
                user.Id,
                itemId
            );
            var result = await _craftItemService.Delete(itemId);
            if (result) {
                _logger.LogInformation("Item {id} deleted", itemId);
                return NoContent();
            }
            _logger.LogError("Fail on delete item {id}", itemId);
            return BadRequest(new ApiError($"Fail on delete this item {itemId}"));
        }
        catch (ServiceException ex) {
            _logger.LogError("Service error on delete item {id}: {message}", itemId, ex.Message);
            return StatusCode(
                (int)HttpStatusCode.BadRequest, 
                new ApiError(ex.Message)
            );
        }
        catch (Exception ex) {
            _logger.LogError("Unknowed error: {message}\n {stack}",
                ex.Message,
                ex.StackTrace
            );
            return StatusCode(
                (int)HttpStatusCode.InternalServerError, 
                new ApiError($"Unkonwed error while trying to delete item {itemId}")
            );
        }
    }

    [Authorize(JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] NewItemData data) {
        string itemAsJson = JsonSerializer.Serialize(data);

        CraftCategory? category = null;
        if (data.categoryId is not null) {
            category = await _db.CraftCategories
                .Where(c => c.Id == data.categoryId)
                .SingleOrDefaultAsync();
            if (category is null) {
                _logger.LogError("Category {id} not found when trying to add item {data}", 
                    data.categoryId, 
                    itemAsJson
                );
                return NotFound(new ApiError("Category " + data.categoryId + " not found."));
            }

        }
        Asset? asset = null;
        if (data.assetId is not null) {
            asset = await _db.Assets
                .Where(a => a.Id == data.assetId)
                .SingleOrDefaultAsync();
            if (asset is null) {
                _logger.LogError("Asset {id} not found when trying to add item {data}", 
                    data.assetId, 
                    itemAsJson
                );
                return NotFound(new ApiError("Asset " + data.assetId + " not found."));
            }
        }
        List<Db.Attribute> attributes = _db.Attributes
            .Where(a => data.attributes
                .Select(at => at.attributeId).Contains(a.Id))
            .ToList();
        if (attributes.Count != data.attributes.Count) {
            var notFoundAttributes = data.attributes.ExceptBy(
                attributes.Select(a => a.Id),
                a => a.attributeId
            );
            _logger.LogError("Not all attributes found in the database - attributes not found: {attributes} - item: {item}", 
                JsonSerializer.Serialize(notFoundAttributes),
                itemAsJson
            );
            return BadRequest("Not all attributes were found in the database.");
        }
        CraftItem craftItem = new() {
            Level = data.level,
            Name = data.name
        };
        if (asset is not null) {
            craftItem.Asset = asset;
            craftItem.AssetId = asset.Id;
        }
        if (category is not null) {
            craftItem.Category = category;
            craftItem.CategoryId = category.Id;
        }
        List<CraftItemAttribute> itemAttributes = data.attributes
            .Select(a => 
                new CraftItemAttribute() {
                    Attribute = attributes.First(att => att.Id == a.attributeId),
                    AttributeId = a.attributeId,
                    Item = craftItem,
                    Value = a.value
                }
            ).ToList();
        craftItem.Attributes = itemAttributes;
        _logger.LogInformation("Adding item {item}", itemAsJson);
        bool itemWasAdded = await _craftItemService.Add(craftItem);
        if (!itemWasAdded) {
            _logger.LogError("Fail on add the item {item}", itemAsJson);
            return BadRequest(new ApiError("Error on adding the item."));
        }
        _logger.LogInformation("Item added: {item}", itemAsJson);
        return Created("", craftItem);
    }
}