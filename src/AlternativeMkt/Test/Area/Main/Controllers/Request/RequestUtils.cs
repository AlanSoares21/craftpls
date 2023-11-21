
using AlternativeMkt;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Controllers;
using AlternativeMkt.Tests;
using Microsoft.Extensions.Logging;

public class RequestUtils
{
    protected IAuthService AuthServiceWithUser(User user) {
        return Utils.AuthServiceWithUser(user);
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
        Utils.SetControllerClaims(controller);
        return controller;
    }

    ILogger<RequestController> GetLogger() => 
        new Mock<ILogger<RequestController>>().Object;
}