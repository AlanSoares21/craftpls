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

    public static CraftResource GetCraftResource(
        int craftRersourceId, 
        CraftItem item, 
        CraftItem resourceItem) {    
        CraftResource craftResource = new() {
            Id = craftRersourceId,
            Amount = 1,
            ItemId = item.Id,
            Item = item,
            ResourceId = resourceItem.Id,
            Resource = resourceItem
        };
        resourceItem.ResourceFor.Add(craftResource);
        item.Resources.Add(craftResource);
        return craftResource;
    }

    public static CraftItemsPrice GetPrice(int price, int totalPrice, CraftItem item, Manufacturer manufacturer) {
        CraftItemsPrice craftPrice = new() {
            Id = Guid.NewGuid(),
            Manufacturer = manufacturer,
            ManufacturerId = manufacturer.Id,
            Item = item,
            ItemId = item.Id,
            Price = price,
            TotalPrice = totalPrice
        };
        item.Prices.Add(craftPrice);
        manufacturer.CraftItemsPrices.Add(craftPrice);
        return craftPrice;
    }

    public static Manufacturer GetManufacturer() {
        return GetManufacturer(new User() {
            Id = Guid.NewGuid()
        });
    }

    public static Manufacturer GetManufacturer(User user) {
        Manufacturer manufacturer = new() {
            Id = Guid.NewGuid(),
            Userid = user.Id,
            User = user
        };
        return manufacturer;
    }
}