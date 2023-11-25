using System.Linq.Expressions;
using AlternativeMkt.Admin.Models;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
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
    public CraftResourcesController(
        MktDbContext db,
        ILogger<CraftResourcesController> logger,
        IAuthService auth) {
        _db = db;
        _logger = logger;
        _auth = auth;
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

    /*

    [HttpPost("{id}")]
    public IActionResult Update(int id, [FromBody] AddResource resourceData) {
        var resource = _db.CraftResources.Where(r => r.Id == id).Single();

        resource.Amount 
        pega a diferenca do q era pro q vai ser;

        atualiza a quantidade

        atualiza os preços para bater com a nova quantidade;
        
        CraftItem item = ;
        prices = _db.CraftItemsPrices.Where(p => p.ItemId == item.Id);
        altera do price a mudança na quantidade de resource

        _db.SaveChangesAsync()
        return Created();
    }
    */

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) {
        User user = _auth.GetUser(User.Claims);
        string logPrefix = $"User {user} tried delete craft resource {id}";
        var craftResource = await _db.CraftResources
            .Where(r => r.Id == id)
            .SingleOrDefaultAsync();
        
        if (craftResource is null) {
            _logger.LogError("{prefix} but the record could not be found in the database", 
                logPrefix
            );
            return NotFound(new ApiError($"Craft resource {id} could not be found in the database"));
        }
        // TODO:: adicionar lógica para remover o valor do resource dos preços antigos
        _db.CraftResources.Remove(craftResource);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}