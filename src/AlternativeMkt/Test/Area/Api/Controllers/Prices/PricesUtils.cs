
using AlternativeMkt.Api.Controllers;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using Microsoft.Extensions.Logging;

namespace AlternativeMkt.Tests.Api.Controllers;

public class PricesUtils
{
    protected PricesController GetController(MktDbContext context, IAuthService auth) {
        var controller = new PricesController(context, GetLogger(), auth);
        Utils.SetControllerClaims(controller);
        return controller;
    }

    ILogger<PricesController> GetLogger() => new Mock<ILogger<PricesController>>().Object;
}