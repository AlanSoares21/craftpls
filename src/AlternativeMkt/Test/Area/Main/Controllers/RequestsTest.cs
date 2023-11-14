using System.Linq.Expressions;
using System.Security.Claims;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Controllers;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AlternativeMkt.Tests.Main.Controller;

public class RequestsTest
{

    /*
        Open/itemPriceId -> mostra o item, os recursos necessários,
        o preço do manufacturer para cada recurso e 
        o preço total do request

        New -> cria request, trata erros

        Show(id) -> mostra dados do request, permite editar(*)

        List -> lista os requests onde o usuário é o requester
    */

    [Fact]
    public void Open_Request()
    {
        var id = Guid.NewGuid();
        var itemPrice = new CraftItemsPrice() {Id = id};
        var priceList = new List<CraftItemsPrice>() { itemPrice };

        var mockDb = new MktDbContextBuilder()
            .WithItemsPrices(priceList)
            .Build();
            
        var controller = GetController(
            mockDb.Object,
            new Mock<IAuthService>().Object
        );
        var result = controller.Open(id) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Open", result.ViewName);

        var model = (CraftItemsPrice?) result.Model;
        Assert.NotNull(model);
        Assert.Equal(itemPrice, model);
    }

    [Fact]
    public void When_Item_Price_Not_Found_Return_Error_Page()
    {
        var priceList = new List<CraftItemsPrice>() { };
        
        var mockDb = new MktDbContextBuilder()
            .WithItemsPrices(priceList)
            .Build();
        var controller = GetController(
            mockDb.Object,
            new Mock<IAuthService>().Object
        );
        var result = controller.Open(Guid.NewGuid()) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Error", result.ViewName);

        var model = (string?) result.Model;
        Assert.Contains("Price not found", model);
    }

    [Fact]
    public async Task When_Item_Price_Not_Found_Return_Error() {
        var mockAuth = new Mock<IAuthService>();
        mockAuth.Setup(a => a.GetUser(It.IsAny<IEnumerable<Claim>>()))
            .Returns(new User());

        var priceList = new List<CraftItemsPrice>() { };
        var mockDb = new MktDbContextBuilder()
            .WithItemsPrices(priceList)
            .Build();
        var controller = GetController(mockDb.Object, mockAuth.Object);
        SetClaims(controller);
        var result = await controller.New(new()) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Error", result.ViewName);
        var model = (string?) result.Model;
        Assert.Contains("Price not found", model);
    }

    [Fact]
    public async Task When_Requester_Is_The_Manufacturer_Return_Error() {
        User requester = new() {
            Id = Guid.NewGuid()
        };

        CraftItemsPrice itemsPrice = new() {
            ManufacturerId = requester.Id
        };
        var priceList = new List<CraftItemsPrice>() { itemsPrice };
        var mockDb = new MktDbContextBuilder()
            .WithItemsPrices(priceList)
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(requester));
        SetClaims(controller);
        var result = await controller.New(new()) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Error", result.ViewName);
        var model = (string?) result.Model;
        Assert.Contains("You can not request items to your self", model);
    }

    [Fact]
    public async Task Save_New_Request_In_Db() {
        User requester = new() {
            Id = Guid.NewGuid()
        };
        CraftItemsPrice itemsPrice = new() {
            Id = Guid.NewGuid(),
            ManufacturerId = Guid.NewGuid()
        };
        var mockRequests = MktDbContextBuilder.MockDbSet(new List<Request>());

        var mockDb = new MktDbContextBuilder()
            .WithItemsPrices(new List<CraftItemsPrice>() { itemsPrice })
            .WithRequests(mockRequests)
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(requester));
        SetClaims(controller);
        await controller.New(new() {
            PriceId = itemsPrice.Id
        });
        
        mockRequests.Verify(
            m => 
                m.AddAsync(It.IsAny<Request>(), 
                It.IsAny<CancellationToken>()), 
            Times.Once()
        );
        mockDb.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once()
        );
    }

    [Fact]
    public async Task Redirect_To_Show_Action_After_Create_New_Request() {
        User requester = new() {
            Id = Guid.NewGuid()
        };
        var priceList = new List<CraftItemsPrice>() { };
        var mockDb = new MktDbContextBuilder()
            .WithItemsPrices(priceList)
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(requester));
        SetClaims(controller);
        var result = await controller.New(new()) as ViewResult;
        Assert.NotNull(result);
    }

    IAuthService AuthServiceWithUser(User user) {
        var mockAuth = new Mock<IAuthService>();
        mockAuth.Setup(a => a.GetUser(It.IsAny<IEnumerable<Claim>>()))
            .Returns(user);
        return mockAuth.Object;
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
    
    RequestController GetController(MktDbContext db, IAuthService auth) =>
        new RequestController(
            db, 
            auth,
            GetLogger()
        );

    ILogger<RequestController> GetLogger() => 
        new Mock<ILogger<RequestController>>().Object;
}