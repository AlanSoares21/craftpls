using AlternativeMkt.Api.Models;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Api.Controllers;

public class LoginController: BaseApiController
{
    MktDbContext _db;
    IAuthService _auth;
    ServerConfig _config;
    public LoginController(
        MktDbContext db,
        IAuthService auth,
        ServerConfig config
    ) {
        _db = db;
        _auth = auth;
        _config = config;
    }
    [HttpPost]
    public IActionResult Auth([FromBody] LoginData data) {
        if (string.IsNullOrEmpty(data.email))
            return BadRequest(new ApiError("Email must be provided."));
        var user = _db.Users
            .Include(u => u.Roles)
            .Where(u => u.DeletedAt == null && u.Email == data.email)
            .SingleOrDefault();
        if (user is null)
            return NotFound(new ApiError($"User {data.email} not found"));
        if (!user.Roles.Any(r => r.RoleId == _config.DevRoleId))
            return Unauthorized(new ApiError($"User {data.email} dont have permisson to login in the api"));
        var accessToken = _auth.CreateAccessToken(user, out DateTime expiresIn);
        return Ok(new {
            access_token = accessToken,
            user,
            expiresIn
        });
    }
}