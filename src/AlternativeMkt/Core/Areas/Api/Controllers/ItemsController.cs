using System.Linq.Expressions;
using System.Net;
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
        return Ok(_craftItemService.SearchItems(query));
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
}