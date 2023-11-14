
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Main.Controllers;

public class RequestController: BaseController
{
    MktDbContext _db;
    IAuthService _auth;
    ILogger<RequestController> _logger;
    public RequestController(
        MktDbContext db,
        IAuthService auth,
        ILogger<RequestController> logger
    ) {
        _db = db;
        _auth = auth;
        _logger = logger;
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
        int totalPrice = price.Price;
        Request request = new() {
            ItemId = price.ItemId,
            Price = totalPrice,
            RequesterId = user.Id,
            ManufacturerId = price.ManufacturerId
        };
        await _db.Requests.AddAsync(new());
        await _db.SaveChangesAsync();
        // registrar novo request no db
        // saveChanges is called
        // redirect to List Action
        return View();
    }
}