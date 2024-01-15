
using System.Net;
using AlternativeMkt.Auth;
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
        if (RedirectToRefresh(context)) {
            string culture = "en";
            if (context.Request.Path.HasValue && context.Request.Path.Value.Length >= 8)
                culture = context.Request.Path.Value.Substring(6, 2);
            QueryString query = new QueryString()
                .Add("endpoint", context.Request.Path.Value + context.Request.QueryString.ToString());
            string redirectTo = $"/main/{culture}{_refreshTokensRoute}{query}";
            _logger.LogTrace("Redirect user from {path} to refresh route {route} - base: {base} - query: {query}", 
                context.Request.Path, 
                redirectTo,
                context.Request.GetDisplayUrl(),
                context.Request.QueryString
            );
            context.Response.Redirect(redirectTo);
        } else {
            await next.Invoke(context);
        }
    }

    bool RedirectToRefresh(HttpContext context) {
        
        if (
            !context.Request.Path.HasValue || 
            !context.Request.Path.Value.StartsWith("/main") ||
            context.Request.Path.Value.EndsWith(_refreshTokensRoute) ||
            context.Request.Cookies["Authenticated"] != "y"
        )
            return false;
        _logger.LogTrace("Auth middleware on path: {path}", context.Request.Path);
        string? expiresCookie = context.Request.Cookies["ExpiresIn"];
        _logger.LogTrace("Expire cookie: {value}", expiresCookie);
        if (string.IsNullOrEmpty(expiresCookie))
            return false;
        if(!DateTime.TryParse(expiresCookie, out DateTime expires)) {
            _logger.LogTrace("Fail on try parse expiresIn: {value}", expiresCookie);
            return false;
        }
        if(expires > DateTime.UtcNow)
            return false;
        return true;
    }
}