
using System.Reflection.Metadata;
using System.Text.Json;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using AlternativeMkt.Main.Models.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace AlternativeMkt.Main.Controllers;

public class AccountController: BaseController
{
    ILogger<AccountController> _logger;
    readonly IConfiguration _config;
    MktDbContext _db;
    IAuthService _auth;
    public AccountController(
        ILogger<AccountController> logger,
        IConfiguration config,
        MktDbContext db,
        IAuthService auth) {
        _logger = logger;
        _config = config;
        _db = db;
        _auth = auth;
    }
    public IActionResult OAuth() {
        const string googleUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        string url = $"{googleUrl}{GetGoogleOAuthQuery()}";
        return Redirect(url);
    }

    string GetGoogleOAuthQuery() {
        string? googleClientId = _config["Google:ClientId"];
        if (googleClientId is null)
            throw new Exception("Fail to get google client id");
        string? clientOrigin = _config["ClientOrigin"];
        if (clientOrigin is null)
            throw new Exception("Fail to get client origin");
        QueryBuilder builder = new();
        builder.Add("client_id", googleClientId);
        builder.Add("redirect_uri", $"{clientOrigin}{Url.Action("Login")}");
        builder.Add("response_type", "token");
        builder.Add("scope", "https://www.googleapis.com/auth/userinfo.email");
        return builder.ToString();
    }

    public async Task<IActionResult> Login([FromQuery] string? access_token) {
        if (access_token is null || access_token.Length == 0)
            return View();
        _logger.LogInformation("new login");
        string? email = await GetUserMail(access_token);
        if (email is null) {
            ViewData["ErrorMessage"] = "Error on retrive user data";
            return View("Error");
        }
        _logger.LogInformation("Success on get user email {email}", email);
        return await AuthenticateUserSession(email);
    }

    async Task<string?> GetUserMail(string token) {
        const string apiUrl = "https://www.googleapis.com/oauth2/v3/userinfo";
        
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders
                .Add("Authorization", $"Bearer {token}");
            HttpResponseMessage response = await client.GetAsync(apiUrl);   
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var userData = JsonSerializer.Deserialize<Userinfo>(content);
                if (userData is not null)
                    return userData.email;
                _logger.LogError("Fail on deserialize response to get user data. status: {code}. response: {content}", response.StatusCode, content);
            }
            else
                _logger.LogError("Error on validate access token. Code {code}", response.StatusCode);
        }
        return null;
    }

    async Task<IActionResult> AuthenticateUserSession(string email) {
        string redirectTo = "/";
        User? user = await _db.Users
            .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(u => u.Email == email);
        if (user is null) {
            await CreateNewUser(email);
            user = await _db.Users.SingleAsync(u => u.Email == email);
            redirectTo = Url.Action("Profile") + "";
        }
        string refreshToken = _auth.CreateRefreshToken();
        await _auth.StoreRefreshToken(user.Id, refreshToken);
        string accessToken = _auth.CreateAccessToken(user, out DateTime expiresIn);
        SetUserCookies(accessToken, refreshToken, expiresIn);
        return Redirect(redirectTo);
    }

    async Task CreateNewUser(string email) {
        await _db.Users.AddAsync(new(){
            Email = email
        });
        await _db.SaveChangesAsync();
    }

    void SetUserCookies(string access_token, string refreshToken, DateTime accessTokenExpiresIn) {
        Response.Cookies.Append("Identifier", access_token);
        Response.Cookies.Append("Reauth", refreshToken);
        Response.Cookies.Append("ExpiresIn", accessTokenExpiresIn.ToString());
        _logger.LogInformation("Set authenticated cookie to true");
        Response.Cookies.Append("Authenticated", "y");
    }


    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Profile(
        [FromServices] IAuthorizationService authorizationService
    ) {
        Task<AuthorizationResult> checkAdminAccess = 
            authorizationService.AuthorizeAsync(User, "AdminAccess");
        var authData = _auth.GetUser(User.Claims);
        var userData = await _db.Users
            .Include(u => u.GameAccounts)
                .ThenInclude(g => g.Server)
            .SingleOrDefaultAsync(u => u.Id == authData.Id);
        if (userData is null)
            throw new Exception("User data ta nulo");
        if (userData.GameAccounts is null)
            throw new Exception("User game accounts ta nulo");
        await checkAdminAccess;
        ViewData["AdminAccess"] = checkAdminAccess.Result.Succeeded;
        return View(userData);
    }

    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult EditProfile() {
        _logger.LogInformation(
            "Edit profile of {name}", 
            User.Identity != null ? User.Identity.Name : "unknow"
        );
        User userData = _auth.GetUser(User.Claims);
        _logger.LogInformation(
            "Data of user {id} - {email}", 
            userData.Id,
            userData.Email
        );
        return View(userData);
    }

    [HttpPost]
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Edit([Bind("name")] EditUser data) {
        _logger.LogInformation(
            "Editing profile data of {id}", 
            User.Identity != null ? User.Identity.Name : "unknow"
        );
        var authData = _auth.GetUser(User.Claims);
        var user = await _db.Users.SingleAsync(u => u.Id == authData.Id);
        if (user.Email is null)
            throw new Exception($"User {authData.Id} email is null");
        user.Name = data.name;
        user.UpdatedAt = new(DateTime.UtcNow.Ticks);
        _logger.LogInformation("Updating user name: {name}", user.Name);
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        _logger.LogInformation("User name updated - id: {id}", authData.Id);
        await AuthenticateUserSession(user.Email);
        return RedirectToAction(nameof(Profile));
    }
}