
using System.Net;
using System.Security.Claims;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Extensions;

namespace AlternativeMkt.Middlewares;

public class AuthMiddleware : IMiddleware
{
    IAuthService _auth;
    ILogger<AuthMiddleware> _logger;
    const string _refreshTokensRoute = "/Account/RefreshLogin";
    public AuthMiddleware(IAuthService auth, ILogger<AuthMiddleware> logger) {
        _auth = auth;
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (await ShouldRedirect(context)) {
            context.Response.Redirect(context.Request.Path + context.Request.QueryString.ToString(), 
                permanent: false, 
                preserveMethod: true
            );
            return;
        }
        await next.Invoke(context);
    }

    async Task<bool> ShouldRedirect(HttpContext context) {
        if (!ShouldRunAuthcheck(context))
            return false;
        if (!AccessTokenExpired(context))
            return false;

        if (
            !context.Request.Cookies.TryGetValue("Identifier", out string? accessToken) || accessToken == null
            ||
            !context.Request.Cookies.TryGetValue("Reauth", out string? refreshToken) || refreshToken == null
        ) {
            _logger.LogError("Access token has expired but it was not possible to recover the authentication tokens.");
            DontRunAuthCheck(context);
            return false;
        }
        try {
            User userData = _auth.GetUserFromAccessToken(accessToken);
            _logger.LogTrace("Reauthenticating user {email} ({id}) - path {path}", 
                userData.Email, 
                userData.Id, 
                context.Request.Path
            );
            UserAuthData authData = await _auth.GetUserAuthenticated(userData.Id);
            if (!RefreshTokenIsValid(authData, ref refreshToken)) {
                DontRunAuthCheck(context);
                return false;
            }
            string newAccessToken = _auth.CreateAccessToken(userData, out DateTime expiresIn);
            await _auth.StoreRefreshToken(userData.Id, refreshToken);
            SetUserCookies(context, newAccessToken, refreshToken, expiresIn);
            _logger.LogTrace("User {email}({id}) reauthenticated until {date}", userData.Email, userData.Id, expiresIn);
            return true;
        }
        catch (Exception ex) {
            _logger.LogError("Error while trying to reauthenticate a user. Error: {message} \n {stack}",
                ex.Message,
                ex.StackTrace
            );
            DontRunAuthCheck(context);
            return false;
        }
    }

    bool ShouldRunAuthcheck(HttpContext context) => context.Request.Cookies["Authenticated"] == "y";
    bool AccessTokenExpired(HttpContext context) => !DateTime.TryParse(context.Request.Cookies["ExpiresIn"], out DateTime expires)
        || expires <= DateTime.UtcNow;

    void DontRunAuthCheck(HttpContext context) {
        _logger.LogTrace("Set authenticated cookie to false");
        context.Response.Cookies.Append("Authenticated", "n");
    }

    bool RefreshTokenIsValid(UserAuthData data, ref string refreshToken) {
        if (refreshToken != data.RefreshToken) {
            _logger.LogError("User {id} tried reauthenticate but the refresh token sent is different from the registered refresh token",
                data.Id                    
            );
            return false;
        }
        if (data.RefreshTokenExpiryTime < DateTime.UtcNow) {
            _logger.LogError("User {id} session expired", data.Id);
            return false;
        }
        return true;
    }

    void SetUserCookies(HttpContext context, string access_token, string refreshToken, DateTime accessTokenExpiresIn) {
        context.Response.Cookies.Append("Identifier", access_token);
        context.Response.Cookies.Append("Reauth", refreshToken);
        context.Response.Cookies.Append("ExpiresIn", accessTokenExpiresIn.ToString());
    }
}