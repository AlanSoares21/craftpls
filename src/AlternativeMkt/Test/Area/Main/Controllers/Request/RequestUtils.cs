
using System.Security.Claims;
using AlternativeMkt;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

public class RequestUtils
{
    protected IAuthService AuthServiceWithUser(User user) {
        var mockAuth = new Mock<IAuthService>();
        mockAuth.Setup(a => a.GetUser(It.IsAny<IEnumerable<Claim>>()))
            .Returns(user);
        return mockAuth.Object;
    }

    protected RequestController GetController(MktDbContext db, IAuthService auth) =>
        GetController(db, auth, Mock.Of<IDateTools>());

    protected RequestController GetController(MktDbContext db, IAuthService auth, IDateTools date) {
        var controller = new RequestController(
            db, 
            auth,
            GetLogger(),
            date
        );
        SetClaims(controller);
        return controller;
    }

    void SetClaims(RequestController controller) {
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

    ILogger<RequestController> GetLogger() => 
        new Mock<ILogger<RequestController>>().Object;
}