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
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AlternativeMkt.Tests.Main.Controllers;

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
    public async Task Redirect_To_List_After_Create_New_Request() {
        User requester = new() {
            Id = Guid.NewGuid()
        };
        CraftItemsPrice itemsPrice = new() {
            Id = Guid.NewGuid(),
            ManufacturerId = Guid.NewGuid()
        };
        var mockRequests = MktDbContextBuilder.MockDbSet(new List<Request>());
        mockRequests.Setup(r => r.AddAsync(
                It.IsAny<Request>(), 
                It.IsAny<CancellationToken>()
            )
        );
        var mockDb = new MktDbContextBuilder()
            .WithItemsPrices(new List<CraftItemsPrice>() { itemsPrice })
            .WithRequests(mockRequests)
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(requester));
        SetClaims(controller);
        var result = await controller.New(new() {
            PriceId = itemsPrice.Id
        }) as RedirectToActionResult;
        Assert.NotNull(result);
        Assert.Equal("List", result.ActionName);
    }

    [Fact]
    public async Task When_Canceling_A_Request_Not_Requested_By_The_User_Return_Error() {
        User requester = new() {
            Id = Guid.NewGuid()
        };
        Request request = new() {
            Id = Guid.NewGuid(),
            RequesterId = Guid.NewGuid()
        };
        var mockDb = new MktDbContextBuilder()
            .WithRequests(new List<Request>() { request })
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(requester));
        SetClaims(controller);
        var result = await controller.Cancel(request.Id) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Error", result.ViewName);
        Assert.Contains("Request not found", (string?)result.Model);
    }

    [Fact]
    public async Task Can_Not_Cancel_A_Request_Already_Cancelled() {
        User requester = new() {
            Id = Guid.NewGuid()
        };
        Request request = new() {
            Id = Guid.NewGuid(),
            RequesterId = requester.Id,
            Cancelled = DateTime.Now
        };
        var mockDb = new MktDbContextBuilder()
            .WithRequests(new List<Request>() { request })
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(requester));
        SetClaims(controller);
        var result = await controller.Cancel(request.Id) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Error", result.ViewName);
        Assert.Contains("Request already cancelled", (string?)result.Model);
    }

    [Fact]
    public async Task Can_Not_Cancel_A_Request_Already_Accepeted() {
        User requester = new() {
            Id = Guid.NewGuid()
        };
        Request request = new() {
            Id = Guid.NewGuid(),
            RequesterId = requester.Id,
            Accepted = DateTime.Now
        };
        var mockDb = new MktDbContextBuilder()
            .WithRequests(new List<Request>() { request })
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(requester));
        SetClaims(controller);
        var result = await controller.Cancel(request.Id) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Error", result.ViewName);
        Assert.Contains("Request already accepted", (string?)result.Model);
    }

    [Fact]
    public async Task Cancel_Request() {
        User requester = new() {
            Id = Guid.NewGuid()
        };
        Request request = new() {
            Id = Guid.NewGuid(),
            RequesterId = requester.Id,
        };
        var mockDate = new Mock<IDateTools>();
        DateTime updateDate = DateTime.Now;
        mockDate.Setup(d => d.UtcNow()).Returns(updateDate);
        var mockRequests = MktDbContextBuilder
            .MockDbSet(new List<Request>() { request });
        var mockDb = new MktDbContextBuilder()
            .WithRequests(mockRequests)
            .Build();
        var controller = GetController(
            mockDb.Object, 
            AuthServiceWithUser(requester),
            mockDate.Object
        );
        SetClaims(controller);
        await controller.Cancel(request.Id);
        Assert.Equal(updateDate, request.UpdatedAt);
        Assert.Equal(updateDate, request.Cancelled);
        mockRequests.Verify(r => r.Update(request), Times.Once());
        mockDb.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task After_Cancel_Request_Redirect_To_Show_Action() {
        User requester = new() {
            Id = Guid.NewGuid()
        };
        Request request = new() {
            Id = Guid.NewGuid(),
            RequesterId = requester.Id,
        };
        var mockDate = new Mock<IDateTools>();
        DateTime updateDate = DateTime.Now;
        mockDate.Setup(d => d.UtcNow()).Returns(updateDate);
        var mockRequests = MktDbContextBuilder
            .MockDbSet(new List<Request>() { request });
        var mockDb = new MktDbContextBuilder()
            .WithRequests(mockRequests)
            .Build();
        var controller = GetController(
            mockDb.Object, 
            AuthServiceWithUser(requester),
            mockDate.Object
        );
        SetClaims(controller);
        var result = await controller
            .Cancel(request.Id) as RedirectToActionResult;
        Assert.NotNull(result);
        Assert.Equal("Show", result.ActionName);
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
        GetController(db, auth, Mock.Of<IDateTools>());

    RequestController GetController(MktDbContext db, IAuthService auth, IDateTools date) =>
        new RequestController(
            db, 
            auth,
            GetLogger(),
            date
        );

    ILogger<RequestController> GetLogger() => 
        new Mock<ILogger<RequestController>>().Object;
}