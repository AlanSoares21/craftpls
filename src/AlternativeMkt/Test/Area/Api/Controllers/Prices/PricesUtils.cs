
using AlternativeMkt.Api.Controllers;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Services;
using Microsoft.Extensions.Logging;

namespace AlternativeMkt.Tests.Api.Controllers;

public class PricesUtils
{
    protected PricesController GetController(
        MktDbContext context, 
        IAuthService auth
        ) =>
        GetController(
            context, 
            auth, 
            new PriceServiceBuilder()
                .WithDb(context)
                .Build()
        );
    protected PricesController GetController(
        MktDbContext context, 
        IAuthService auth,
        IPriceService priceService
    ) {
        var controller = new PricesController(
            context, 
            GetLogger(), 
            auth,
            priceService
        );
        Utils.SetControllerClaims(controller);
        return controller;
    }

    ILogger<PricesController> GetLogger() => new Mock<ILogger<PricesController>>().Object;
}