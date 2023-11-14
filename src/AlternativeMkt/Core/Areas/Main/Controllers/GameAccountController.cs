using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Main.Controllers;

[Authorize]
public class GameAccountController: BaseController
{
    readonly MktDbContext _db;
    ILogger<GameAccountController> _logger;
    IAuthService _auth;
    public GameAccountController(
        MktDbContext db,
        ILogger<GameAccountController> logger,
        IAuthService auth
    ) {
        _db = db;
        _logger = logger;
        _auth = auth;
    }
    public IActionResult New() {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register([Bind("name,server")] NewGameAccount data) {
        var user = _auth.GetUser(User.Claims);
        var server = _db.Servers.SingleOrDefault(s => s.Id == data.server);
        if (server is null) {
            _logger.LogWarning("On trying to add account {name} to user {user}, server {id} not found",
                data.name,
                user.Id,
                data.server
            );
            return View("Error", $"Server with id {data.server} not found");
        }
        GameAccount newAccount = new() {
            CreatedAt = new(DateTime.UtcNow.Ticks),
            Name = data.name,
            ServerId = data.server,
            UserId = user.Id
        };
        try {
            _logger.LogInformation("Adding account {name}(server: {server}) to user {user}",
                newAccount.Name,
                newAccount.ServerId,
                newAccount.UserId
            );
            var result = _db.GameAccounts.Add(newAccount);
            await _db.SaveChangesAsync();
        } catch (DbUpdateException ex) {
            _logger.LogError("Error when trying add account {n}(server: {s}) for user {u} - Message: {m} - InnerException: {i}", 
                newAccount.Name,
                newAccount.ServerId,
                newAccount.UserId, 
                ex.Message,
                ex.InnerException?.Message
            );
            return View("Error", $"Error on add account {newAccount.Name} from server {server.Name}. Try again later.");
        } catch(Exception ex) {
            _logger.LogCritical("Unknowed error when trying add account {n}(server: {s}) for user {u} - Message: {m} - InnerException: {i}", 
                newAccount.Name,
                newAccount.ServerId,
                newAccount.UserId, 
                ex.Message,
                ex.InnerException?.Message
            );
            return View("Error", $"A critical error ocurred. Try again later.");
        }
        _logger.LogInformation("Account {name}(server: {server}) added to user {user}",
            newAccount.Name,
            newAccount.ServerId,
            newAccount.UserId
        );
        return RedirectToAction("Profile", "Account");
    }
}