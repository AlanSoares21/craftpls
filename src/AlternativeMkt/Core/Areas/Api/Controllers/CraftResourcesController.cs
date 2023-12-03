using System.Linq.Expressions;
using AlternativeMkt.Admin.Models;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using AlternativeMkt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Api.Controllers;

[Authorize(Policy = "AdminAccess")]
public class CraftResourcesController: BaseApiController
{
    MktDbContext _db;
    ILogger<CraftResourcesController> _logger;
    IAuthService _auth;
    ICraftResourceService _resourceService;
    public CraftResourcesController(
        MktDbContext db,
        ILogger<CraftResourcesController> logger,
        IAuthService auth,
        ICraftResourceService craftResourceService) {
        _db = db;
        _logger = logger;
        _auth = auth;
        _resourceService = craftResourceService;
    }

    // TODO:: impedir de adicionar recursos quando o item já tem preços cadastrados ou
    //        implementar: invalidar preços antigos, para que o preço volte a ser valido
    //        o manufacturer deve registrar um preço para o resource, e então o valor do 
    //        total price do item será recalculado
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddResource resourceData) {
        User user = _auth.GetUser(User.Claims);
        string logPrefix = $"User {user.Id} tried to set item {resourceData.resourceId} as a resource to {resourceData.itemId}";
        if (resourceData.amount < 1) {
            _logger.LogError("{prefix} but the amount is less than one. Ammount: {amount}",
                logPrefix,
                resourceData.amount
            );
            return BadRequest(new ApiError("The amount should be greater than zero"));
        }
        if (resourceData.resourceId == resourceData.itemId) {
            _logger.LogError("{prefix} but the resource and the item should be a different.",
                logPrefix
            );
            return BadRequest(new ApiError("The resource and item should be different items"));
        }
        var (resourceItem, item) =  FindResourceAndItem(resourceData.resourceId, resourceData.itemId);
        if (resourceItem is null) {
            _logger.LogError("{prefix} but resource item {id} not found", 
                logPrefix, 
                resourceData.resourceId
            );
            return NotFound(new ApiError($"Resource item {resourceData.resourceId} not found"));
        }
        if (item is null) {
            _logger.LogError("{prefix} but item {id} not found", 
                logPrefix, 
                resourceData.itemId
            );
            return NotFound(new ApiError($"Item {resourceData.itemId} not found"));
        }
        if (item.Resources.Any(r => r.ResourceId == resourceItem.Id)) {
            _logger.LogError("{prefix} but item {id} already have item {resourceid} as a resource", 
                logPrefix, 
                item.Id,
                resourceItem.Id
            );
            return BadRequest(new ApiError($"This item already have this resource"));
        }
        CraftResource resource = new() {
            ItemId = item.Id,
            ResourceId = resourceItem.Id,
            Amount = resourceData.amount
        };
        _logger.LogInformation("User {user} is adding item {resource} as a resource to item {item} with the amount {amount}",
            user.Id,
            resource.ResourceId,
            resource.ItemId,
            resource.Amount
        );
        await _db.CraftResources.AddAsync(resource);
        await _db.SaveChangesAsync();
        _logger.LogInformation("User {user} added item {resource} as a resource to item {item} with the amount {amount} - resource id: {id}",
            user.Id,
            resource.ResourceId,
            resource.ItemId,
            resource.Amount,
            resource.Id
        );
        return Created("/" + resource.Id, resource);
    }

    (CraftItem? resourceItem, CraftItem? item) FindResourceAndItem(int resourceId, int itemId) {
        var item =  _db.CraftItems
            .Include(i => i.Resources)
            .Where(i => i.Id == itemId || i.Id == resourceId)
            .ToList();
        return (
            resourceItem: item.Find(i => i.Id == resourceId), 
            item: item.Find(i => i.Id == itemId)
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateResource resourceData) {
        User user = _auth.GetUser(User.Claims);
        var craftResource = await _db.CraftResources
            .Include(r => r.Item)
            .Where(r => r.Id == id)
            .SingleOrDefaultAsync();
        if (craftResource is null) {
            _logger.LogError("User {user} tried update craft resource {id} but the record could not be found in the database", 
                user.Id,
                id
            );
            return NotFound(new ApiError($"Craft resource {id} could not be found in the database"));
        }
        _logger.LogInformation("User {user} is updating craft resource {id} - amount: {amount}", 
            user.Id,
            craftResource.Id,
            resourceData.amount
        );
        craftResource.Amount = resourceData.amount;
        await _resourceService.UpdateResource(craftResource);
        _logger.LogInformation("Craft resource {id} updated by user {user} - amount: {amount}", 
            craftResource.Id,
            user.Id,
            resourceData.amount
        );
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) {
        User user = _auth.GetUser(User.Claims);
        var craftResource = await _db.CraftResources
            .Include(r => r.Item)
            .Where(r => r.Id == id)
            .SingleOrDefaultAsync();
        if (craftResource is null) {
            _logger.LogError("User {user} tried delete craft resource {id} but the record could not be found in the database", 
                user.Id,
                id
            );
            return NotFound(new ApiError($"Craft resource {id} could not be found in the database"));
        }
        _logger.LogInformation("User {user} is deleting craft resource {id}", 
            user.Id,
            craftResource.Id
        );
        await _resourceService.DeleteResource(craftResource);
        _logger.LogInformation("Craft resource {id} deleted by user {user}", 
            craftResource.Id,
            user.Id
        );
        return NoContent();
    }
}