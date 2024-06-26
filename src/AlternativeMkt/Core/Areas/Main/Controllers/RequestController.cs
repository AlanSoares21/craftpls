
using System.Runtime.CompilerServices;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Main.Controllers;

[Authorize(JwtBearerDefaults.AuthenticationScheme)]
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
            .Include(p => p.Item)
                .ThenInclude(i => i.Asset)
            .Include(p => p.Item.DataByCulture
                .Where(d => d.Culture == GetCulture()))
            .Include(p => p.Item.Attributes)
                .ThenInclude(a => a.Attribute)
            .Include(p => p.Manufacturer)
                .ThenInclude(m => m.Server)
                    .ThenInclude(s => s.GameAccounts
                        .Where(a => a.Server.Manufacturers
                            .Any(m => m.Userid == a.UserId)
                        )
                    )
            .Where(p => p.Id == id 
                && p.DeletedAt == null 
                && !p.ResourcesChanged
            )
            .SingleOrDefault();
        if (price is null)
            return View("Error", "Price not found");
        var resources = _db.CraftResources
            .Include(r => r.Resource)
                .ThenInclude(i => i.Asset)
            .Include(r => r.Resource.Prices
                .Where(p => p.ManufacturerId == price.ManufacturerId
                    && p.DeletedAt == null
                    && !p.ResourcesChanged
                )
            )
            .Include(r => r.Resource.DataByCulture
                .Where(d => d.Culture == GetCulture()))
            .Where(r => r.ItemId == price.ItemId)
            .ToList();
        price.Item.Resources = resources;
        return View("Open", price);
    }
    
    public async Task<IActionResult> New([Bind("PriceId,ProvidedResources")] NewRequest requestData) {
        // pegar dados do usuario
        var user = _auth.GetUser(User.Claims);
        // verificar se o item price tem um registro no db
        var price = _db.CraftItemsPrices
            .Include(p => p.Item)
                .ThenInclude(i => i.Resources)
            .Include(p => p.Manufacturer)
            .Where(p => 
                p.Id == requestData.PriceId
                && p.DeletedAt == null
                && !p.ResourcesChanged    
            )
            .SingleOrDefault();
        if (price is null) {
            _logger.LogError("User {user} tried request item with price {price}, but the price could not be found in the db",
                user.Id,
                requestData.PriceId
            );
            return View("Error", "Price not found");
        }
        if (price.Manufacturer.Userid == user.Id) {
            _logger.LogError("User {user} tried request item to him self, priceId: {id}",
                user.Id,
                requestData.PriceId
            );
            return View("Error", "You can not request items to your self");
        }
        int requestPrice = price.TotalPrice;
        if (requestData.ProvidedResources.Count > 0) {
            CraftResource[] providedResources = price.Item.Resources
                .Where(r => requestData.ProvidedResources.Contains(r.Id))
                .ToArray();
            if (providedResources.Length != requestData.ProvidedResources.Count) {
                _logger.LogError("User {user} tried request item {item} but one of follow resources {resources} are not in the item resources list.",
                    user.Id,
                    price.ItemId,
                    string.Join(',', requestData.ProvidedResources)
                );
                return View("Error", "Some of the resources provided are not in the resources list.");
            }
            _logger.LogTrace("Sum prices of {count} craft resources provided", 
                providedResources.Length
            );
            int? sum = _db.CraftItemsPrices
                .Include(p => p.Item)
                    .ThenInclude(i => i.ResourceFor
                        .Where(r => r.ItemId == price.ItemId)
                    )
                .Where(p =>
                    p.ManufacturerId == price.ManufacturerId
                    && p.DeletedAt == null
                    && providedResources.Select(rp => rp.ResourceId).Contains(p.ItemId)
                ).Sum(p => p.TotalPrice * p.Item.ResourceFor.First().Amount);
            if (sum is null) {
                _logger.LogError("User {user} tried to request price {price} but the sum of the resources provided prices was null. Resources provided {resources}",
                    user.Id,
                    price.Id,
                    string.Join(',', requestData.ProvidedResources)
                );
                return View("Error", "We had a problem when checking the price of the resources provided. Try again later");
            }
            _logger.LogInformation("User {user} is requesting price {price}. The sum of the resources provided prices is {value}. Resources provided {resources}",
                user.Id,
                price.Id,
                sum,
                string.Join(',', requestData.ProvidedResources)
            );
            requestPrice -= sum.Value;
        }
        _logger.LogInformation("User {user} is requesting item with price {price}",
            user.Id,
            requestData.PriceId
        );
        Request request = new() {
            ItemId = price.ItemId,
            Price = requestPrice,
            RequesterId = user.Id,
            ManufacturerId = price.ManufacturerId,
            ResourcesProvided = price.Item.Resources
                .Where(r => requestData.ProvidedResources.Any(id => id == r.Id ))
                .Select(r => new ResourceProvided() {
                    Resource = r,
                    ResourceId = r.Id
                })
                .ToList()
        };
        await _db.Requests.AddAsync(request);
        await _db.SaveChangesAsync();
        return RedirectToAction("List");
    }
    
    public IActionResult List([FromQuery]StandardPaginationParams query) {
        if (query.count < 1)
            query.count = 10;
        User user = _auth.GetUser(User.Claims);
        StandardList<Request> requests = new() {
            Start = query.start,
            Count = query.count
        };
        requests.Count = query.count;
        requests.Data = _db.Requests
            .Include(r => r.Manufacturer)
                .ThenInclude(m => m.Server)
                    .ThenInclude(s => s.GameAccounts
                        .Where(a => a.Server.Manufacturers
                            .Any(m => m.Userid == a.UserId)
                        )
                    )
            .Include(r => r.Item)
                .ThenInclude(i => i.Asset)
            .OrderByDescending(r => r.CreatedAt)
            .Where(r => r.RequesterId == user.Id && r.DeletedAt == null)
            .Skip(query.start)
            .Take(query.count)
            .ToList();
        requests.Total = _db.Requests
            .Where(r => r.RequesterId == user.Id && r.DeletedAt == null)
            .Count();
        ViewData["Query"] = query;
        return View(requests);
    }
    
    public IActionResult Show(Guid id) {
        User user = _auth.GetUser(User.Claims);
        Request? request = SearchUserRequest(user, id);
        if (request is null)
            return View("Error", "Request not found");
        return View(request);
    }

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
        
        return RedirectToAction("List");
    }

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
        if (request.Accepted is not null) {
            _logger.LogWarning("User {user} tried accept request {id}, but the request was accepted in {d}", 
                user.Id,
                id,
                request.Accepted
            );
            return View("Error", "You can not accept an already accepted request");
        }
        if (request.Refused is not null) {
            _logger.LogWarning("User {user} tried accept request {id}, but the request was refused at {d}", 
                user.Id,
                id,
                request.Refused
            );
            return View("Error", "You can not accept a refused request");
        }
        request.Accepted = _date.UtcNow();
        request.UpdatedAt = request.Accepted;
        _db.Requests.Update(request);
        await _db.SaveChangesAsync();
        return RedirectToAction("Requests", "Manufacturer", new {Id = request.ManufacturerId});
    }

    public async Task<IActionResult> Refuse(Guid id) {
        User user = _auth.GetUser(User.Claims);
        var request = _db.Requests
            .Include(r => r.Manufacturer)
            .Where(r => r.Id == id && r.DeletedAt == null)
            .SingleOrDefault();
        if (request is null) {
            _logger.LogInformation("User {user} tried refuse request {id}, but the request is not in the database", 
                user.Id,
                id
            );
            return View("Error", "Request not found");
        }
        if (request.Manufacturer.Userid != user.Id) {
            _logger.LogWarning("User {user} tried refuse request {id}, but the manufacturer is {manufacturer}", 
                user.Id,
                id,
                request.ManufacturerId
            );
            return View("Error", "You can not refuse a request when you is not the manufacturer");
        }
        if (request.Cancelled is not null) {
            _logger.LogInformation("User {user} tried refuse request {id}, but the request was cancelled in {d}", 
                user.Id,
                id,
                request.Cancelled
            );
            return View("Error", "You can not refuse a cancelled request");
        }
        if (request.Accepted is not null) {
            _logger.LogWarning("User {user} tried refuse request {id}, but the request was accepted in {d}", 
                user.Id,
                id,
                request.Accepted
            );
            return View("Error", "You can not refuse an accepted request");
        }
        if (request.Refused is not null) {
            _logger.LogWarning("User {user} tried refuse request {id}, but the request already was refused at {d}", 
                user.Id,
                id,
                request.Refused
            );
            return View("Error", "You can not refuse a refused request");
        }
        request.Refused = _date.UtcNow();
        request.UpdatedAt = request.Refused;
        _db.Requests.Update(request);
        await _db.SaveChangesAsync();
        return RedirectToAction("Requests", "Manufacturer", new {Id = request.ManufacturerId});
    }

    public async Task<IActionResult> Finished(Guid id) {
        User user = _auth.GetUser(User.Claims);
        var request = _db.Requests
            .Include(r => r.Manufacturer)
            .Where(r => r.Id == id && r.DeletedAt == null)
            .SingleOrDefault();
        if (request is null) {
            _logger.LogError("Error request not found. User {id} tried to finish request {id}",
                user.Id,
                id
            );
            return View("Error", "Request not found");
        }
        if (request.RequesterId != user.Id && request.Manufacturer.Userid != user.Id) {
            _logger.LogError("User {id} is not involved in request {id} but tried finish it",
                user.Id,
                id
            );
            return View("Error", "You can not finish this request");
        }
        if (request.Cancelled is not null) {
            _logger.LogError("User {id} tried to finish request {id} but the request was cancelled",
                user.Id,
                id
            );
            return View("Error", "The request is cancelled");
        }
        if (request.Refused is not null) {
            _logger.LogError("User {id} tried to finish request {id} but the request was refused",
                user.Id,
                id
            );
            return View("Error", "The manufacturer refused this request");
        }
        if (request.Accepted is null) {
            _logger.LogError("User {id} tried to finish request {id} but the request is not accepted yet",
                user.Id,
                id
            );
            return View("Error", "The manufacturer needs accept this request");
        }
        var finishedAt = _date.UtcNow();
        request.UpdatedAt = finishedAt;
        if (request.RequesterId == user.Id)
            request.FinishedByRequester = finishedAt;
        else
            request.FinishedByManufacturer = finishedAt;
        
        _db.Requests.Update(request);
        await _db.SaveChangesAsync();
        if (request.RequesterId == user.Id)
            return RedirectToAction("List");

        return RedirectToAction("Requests", "Manufacturer", new {Id = request.ManufacturerId});
    }
    
    Request? SearchUserRequest(User user, Guid requestId) {
        return _db.Requests
            .Where(r => r.Id == requestId 
                && r.RequesterId == user.Id 
                && r.DeletedAt == null
            ).SingleOrDefault();
    }
}