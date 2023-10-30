
using System.Text.Json;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Controllers;

[Authorize]
public class ManufacturerController: BaseController
{
    ILogger<ManufacturerController> _logger;
    MktDbContext _db;
    IAuthService _auth;

    public ManufacturerController(
        ILogger<ManufacturerController> logger,
        MktDbContext db,
        IAuthService auth
    ) {
        _logger = logger;
        _db = db;
        _auth = auth;
    }
    public async Task<IActionResult> Index(Guid? id = null) {
        var user = _auth.GetUser(User.Claims);
        if (id is null) {
            var manufacturers = _db
                .Manufacturers
                .Include(m => m.Server)
                .Where(m => m.Userid == user.Id).ToList();
            return View("Select", manufacturers);
        }
        Manufacturer? manufacturer = await _db.Manufacturers
            .Include(m => m.Server)
            .Where(m => m.Id == id && m.Userid == user.Id).SingleOrDefaultAsync();
        if (manufacturer is null) {
            _logger.LogInformation("Manufacturer {m} not found in db for user {id}",
                id,
                user.Id
            );
            return View("Error", $"Manufacturer not found, try select one already registered");
        }
        return View(manufacturer);
    }

    public IActionResult New() => View();

    [HttpPost]
    public async Task<IActionResult> Add([Bind("server,maxRequestsAccepted,maxRequestsOpen")] AddManufacturer data) {
        var user = _auth.GetUser(User.Claims);
        string dataAsJson = JsonSerializer.Serialize(data);
        try {
            _logger.LogInformation("Searching manufacturer registers for user {u} in server {s}",
                user.Id,
                data.server
            );
            var manufacturersInTheServer = _db.Manufacturers
                .Where(m => m.Userid  == user.Id && m.ServerId == data.server)
                .Count();
            if (manufacturersInTheServer > 0) {
                _logger.LogInformation("User {user} tryed to add manufacturer in server {s} but already have records in this server - manufactuer count: {count}",
                    user.Id,
                    data.server,
                    manufacturersInTheServer
                );
                return View("Error", "You already is an manufacturer in this server");
            }
            _logger.LogInformation("Adding manufacturer for user {u} - data: {data}",
                user.Id,
                dataAsJson
            );
            Manufacturer manufacturer = new() {
                Userid = user.Id,
                ServerId = data.server,
                MaxRequestsAccepted = data.maxRequestsAccepted,
                MaxRequestsOpen = data.maxRequestsOpen,
                Hide = false
            };
            await _db.Manufacturers.AddAsync(manufacturer);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Added manufacturer for user {u}", user.Id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) {
            _logger.LogCritical("Unkoewed error on adding manufacturer for user {u} - values: {data} - Message: {m} - InnerException: {i}", 
                user.Id,
                dataAsJson, 
                ex.Message,
                ex.InnerException?.Message
            );
            return View("Error", "Unknowed error adding new manufacturer register. Try again later");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(
        Guid id, 
        [Bind("hide,maxRequestsAccepted,maxRequestsOpen")] 
            EditManufacturer data
    ) {
        var user = _auth.GetUser(User.Claims);
        string dataAsJson = JsonSerializer.Serialize(data);
        try {
            _logger.LogInformation("Editing manufacturer {m} for user {u} - values: {data}", 
                id,
                user.Id,
                dataAsJson
            );
            var updatedAt = new DateTime(DateTime.UtcNow.Ticks);
            int affected = await _db.Manufacturers
                .Where(m => m.Id == id && m.Userid == user.Id)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(m => m.MaxRequestsAccepted, data.maxRequestsAccepted)
                    .SetProperty(m => m.MaxRequestsOpen, data.maxRequestsOpen)
                    .SetProperty(m => m.Hide, data.hide)
                    .SetProperty(m => m.UpdatedAt, updatedAt)
                );
            if (affected != 1) {
                _logger.LogError("Error on update manufactuer {manId} for user {user}, no changes made - data: {data}",
                    id,
                    user.Id,
                    dataAsJson
                );
                return View("Error", "Error on update manufacturer data, no changes made, try again later");
            }
            _logger.LogInformation("Manufacturer {manId} updated for user {user} - new data: {data}",
                id,
                user.Id,
                dataAsJson
            );
            return RedirectToAction("Index", new {id});
        }
        catch (DbUpdateException ex) {
            _logger.LogError("Error on editing manufacturer {manufacturer} for user {u} - values: {data} - Message: {m} - InnerException: {i}", 
                id,
                user.Id,
                dataAsJson, 
                ex.Message,
                ex.InnerException?.Message
            );
            return View("Error", $"Error on update data, check the values provided or try again later.");
        }
        catch (Exception ex) {
            _logger.LogCritical("Unkoewed error on editing manufacturer {manufacturer} for user {u} - values: {data} - Message: {m} - InnerException: {i}", 
                id,
                user.Id,
                dataAsJson, 
                ex.Message,
                ex.InnerException?.Message
            );
            return View("Error", $"Unknowed error on update data, try again later.");
        }
    }
    
    public IActionResult Prices(Guid id) => View();
    public IActionResult Requests(Guid id) => View();
}