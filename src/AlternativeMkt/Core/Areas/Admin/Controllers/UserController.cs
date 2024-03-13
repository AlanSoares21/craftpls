
using AlternativeMkt.Admin.Models;
using AlternativeMkt.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Admin.Controllers;

public class UserController : BaseAdminController
{   
    MktDbContext _db;
    IDateTools _date;
    public UserController(MktDbContext db, IDateTools dateTools) {
        _db = db;
        _date = dateTools;
    }
    [HttpPost]
    public async Task<IActionResult> RegisterAccount([Bind("email")] RegisterAccountData data) {
        var request = _db.CreateUserAccountRequests
            .Where(r => r.Email == data.email && r.AcceptedAt == null)
            .SingleOrDefault();
        if (request is null) {
            return View("Error", $"No request found to user {data.email}");
        }   
        var user = _db.Users
            .Where(u => u.Email == data.email)
            .SingleOrDefault();
        if (user is not null) {
            return View("Error", $"User {user.Email} already registered");
        }
        user = new() {
            Email = request.Email
        };
        _db.Users.Add(user);
        request.AcceptedAt = _date.UtcNow();
        _db.CreateUserAccountRequests.Update(request);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index", "Home");
    }
}