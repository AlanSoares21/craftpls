
using System.Runtime.CompilerServices;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Main.Controllers;

public class RequestController: BaseController
{
    MktDbContext _db;
    IAuthService _auth;
    ILogger<RequestController> _logger;
    IDateTools _date;
    public RequestController(
        MktDbContext db,
        IAuthService auth,
        ILogger<RequestController> logger,
        IDateTools date
    ) {
        _db = db;
        _auth = auth;
        _logger = logger;
        _date = date;
    }
    [AllowAnonymous]
    public IActionResult Open(Guid id) {
        var price = _db.CraftItemsPrices
            .Where(p => p.Id == id)
            .SingleOrDefault();
        if (price is null)
            return View("Error", "Price not found");
        return View("Open", price);
    }

    [Authorize]
    public async Task<IActionResult> New([FromBody] NewRequest requestData) {
        // pegar dados do usuario
        var user = _auth.GetUser(User.Claims);
        // verificar se o item price tem um registro no db
        var price = _db.CraftItemsPrices
            .Include(p => p.Item)
            .Where(p => p.Id == requestData.PriceId)
            .SingleOrDefault();
        if (price is null) {
            _logger.LogError("User {user} tried request item with price {price}, but the price could not be found in the db",
                user.Id,
                requestData.PriceId
            );
            return View("Error", "Price not found");
        }
        if (price.ManufacturerId == user.Id) {
            _logger.LogError("User {user} tried request item to him self, priceId: {id}",
                user.Id,
                requestData.PriceId
            );
            return View("Error", "You can not request items to your self");
        }
        _logger.LogInformation("User {user} is requesting item with price {price}",
            user.Id,
            requestData.PriceId
        );
        Request request = new() {
            ItemId = price.ItemId,
            Price = price.TotalPrice,
            RequesterId = user.Id,
            ManufacturerId = price.ManufacturerId
        };
        await _db.Requests.AddAsync(request);
        await _db.SaveChangesAsync();
        return RedirectToAction("List");
    }

    [Authorize]
    public IActionResult List() {
        User user = _auth.GetUser(User.Claims);
        var requests = _db.Requests
            .Where(r => r.RequesterId == user.Id && r.DeletedAt == null)
            .OrderBy(r => r.CreatedAt)
            .ToList();
        return View(requests);
    }

    [Authorize]
    public IActionResult Show(Guid id) {
        User user = _auth.GetUser(User.Claims);
        Request? request = SearchUserRequest(user, id);
        if (request is null)
            return View("Error", "Request not found");
        return View(request);
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Cancel(Guid id) {
        User user = _auth.GetUser(User.Claims);
        Request? request = SearchUserRequest(user, id);
        if (request is null)
            return View("Error", "Request not found");
        if (request.Cancelled is not null) {
            _logger.LogError("User {u} tried cancel request {r} but it is already cancelled",
                user.Id,
                request.Id
            );
            return View("Error", "Request already cancelled");
        }
        if (request.Accepted is not null) {
            _logger.LogError("User {u} tried cancel request {r} but it already was accepted",
                user.Id,
                request.Id
            );
            return View("Error", "Request already accepted");
        }
        DateTime updated = _date.UtcNow();
        request.UpdatedAt = updated;
        request.Cancelled = updated;
        _db.Requests.Update(request);
        await _db.SaveChangesAsync();
        
        return RedirectToAction("Show", id);
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Accept(Guid id) {
        User user = _auth.GetUser(User.Claims);
        var request = _db.Requests
            .Include(r => r.Manufacturer)
            .Where(r => r.Id == id && r.DeletedAt == null)
            .SingleOrDefault();
        if (request is null) {
            _logger.LogInformation("User {user} tried accept request {id}, but the request is not in the database", 
                user.Id,
                id
            );
            return View("Error", "Request not found");
        }
        if (request.Manufacturer.Userid != user.Id) {
            _logger.LogInformation("User {user} tried accept request {id}, but the manufacturer is {manufacturer}", 
                user.Id,
                id,
                request.ManufacturerId
            );
            return View("Error", "You can not accept a request that you is not the manufacturer");
        }
        if (request.Cancelled is not null) {
            _logger.LogInformation("User {user} tried accept request {id}, but the request was cancelled in {d}", 
                user.Id,
                id,
                request.Cancelled
            );
            return View("Error", "You can not accept a cancelled request");
        }
        request.Cancelled = _date.UtcNow();
        _db.Requests.Update(request);
        await _db.SaveChangesAsync();
        return RedirectToAction("Requests", "Manufacturer", request.ManufacturerId);
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Finished(Guid id) {
        User user = _auth.GetUser(User.Claims);
        var request = _db.Requests
            .Where(r => r.Id == id && r.DeletedAt == null)
            .SingleOrDefault();
        if (request is null) {
            _logger.LogError("Error request not found. User {id} tried to finish request {id}",
                user.Id,
                id
            );
            return View("Error", "Request not found");
        }
        if (request.RequesterId != user.Id && request.ManufacturerId != user.Id) {
            _logger.LogError("User {id} is not involved in request {id} but tried finish it",
                user.Id,
                id
            );
            return View("Error", "You can not finish this request");
        }
        if (request.Cancelled is not null) {
            _logger.LogError("User {id} tried to finish request {id} but the request is cancelled",
                user.Id,
                id
            );
            return View("Error", "The request is cancelled");
        }
        var finishedAt = _date.UtcNow();
        request.UpdatedAt = finishedAt;
        if (request.RequesterId == user.Id)
            request.FinishedByRequester = finishedAt;
        else
            request.FinishedByManufacturer = finishedAt;
        
        _db.Requests.Update(request);
        await _db.SaveChangesAsync();
        return RedirectToAction("Requests", "Manufacturer", id);
    }
    
    Request? SearchUserRequest(User user, Guid requestId) {
        return _db.Requests
            .Where(r => r.Id == requestId 
                && r.RequesterId == user.Id 
                && r.DeletedAt == null
            ).SingleOrDefault();
    }
}