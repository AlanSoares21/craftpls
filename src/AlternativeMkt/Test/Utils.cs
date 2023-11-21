using System.Security.Claims;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace AlternativeMkt.Tests;

public static class Utils
{
    public static IAuthService AuthServiceWithUser(User user) {
        var mockAuth = new Mock<IAuthService>();
        mockAuth.Setup(a => a.GetUser(It.IsAny<IEnumerable<Claim>>()))
            .Returns(user);
        return mockAuth.Object;
    }

    public static void SetControllerClaims(ControllerBase controller) {
        var claims = new Claim[] { };
        HttpContext httpContext = new DefaultHttpContext();
        RouteValueDictionary routeDataDictioanry = new();
        RouteData routeData = new RouteData(routeDataDictioanry);
        ControllerActionDescriptor actionDescriptor = 
            new ControllerActionDescriptor();
        ActionContext actionContext = 
            new ActionContext(httpContext, routeData, actionDescriptor);
        actionContext.HttpContext.User = 
            new ClaimsPrincipal(new ClaimsIdentity(claims));
        var controllerContext = new ControllerContext(actionContext);
        controller.ControllerContext = controllerContext;
    }
}