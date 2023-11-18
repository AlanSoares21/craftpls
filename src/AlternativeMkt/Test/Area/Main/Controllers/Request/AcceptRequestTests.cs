using System.Security.Claims;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace AlternativeMkt.Tests.Main.Controllers;

public class AcceptRequestTests
{
    [Fact]
    public async Task When_Not_Found_A_Request_Return_Error() {
        User user = new() {
            Id = Guid.NewGuid()
        };
        Request request = new() {
            Id = Guid.NewGuid(),
            ManufacturerId = Guid.NewGuid()
        };
        var dbMock = new MktDbContextBuilder().Build();
        var controller = GetController(dbMock.Object, AuthServiceWithUser(user));
        SetClaims(controller);
        var result = await controller.Accept(request.Id) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Error", result.ViewName);
        Assert.Equal("Request not found", (string?)result.Model);
    }
    
    [Fact]
    public async Task Can_Not_Accept_A_Request_When_The_User_Is_Not_The_Manufacturer() {
        User user = new() {
            Id = Guid.NewGuid()
        };
        Manufacturer manufacturer = new() {
            Id = Guid.NewGuid(),
            Userid = Guid.NewGuid()
        };
        Request request = new() {
            Id = Guid.NewGuid(),
            ManufacturerId = manufacturer.Id,
            Manufacturer = manufacturer
        };
        var dbMock = new MktDbContextBuilder()
            .WithRequests(new List<Request>() { request })
            .Build();
        var controller = GetController(dbMock.Object, AuthServiceWithUser(user));
        SetClaims(controller);
        var result = await controller.Accept(request.Id) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Error", result.ViewName);
        Assert.Equal("You can not accept a request that you is not the manufacturer", (string?)result.Model);
    }
    
    [Fact]
    public async Task Can_Not_Accept_A_Request_When_It_Is_Cancelled() {
        User user = new() {
            Id = Guid.NewGuid()
        };
        Manufacturer manufacturer = new() {
            Id = Guid.NewGuid(),
            Userid = user.Id
        };
        Request request = new() {
            Id = Guid.NewGuid(),
            ManufacturerId = manufacturer.Id,
            Manufacturer = manufacturer,
            Cancelled = DateTime.Now
        };
        var dbMock = new MktDbContextBuilder()
            .WithRequests(new List<Request>() { request })
            .Build();
        var controller = GetController(dbMock.Object, AuthServiceWithUser(user));
        SetClaims(controller);
        var result = await controller.Accept(request.Id) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Error", result.ViewName);
        Assert.Equal("You can not accept a cancelled request", (string?)result.Model);
    }

    // TODO:: after accept a request redirects to Manufacturer Requests Page
    [Fact]
    public async Task After_Accept_A_Request_Redirects_To_Manufacturer_Request_Page() {
        User user = new() {
            Id = Guid.NewGuid()
        };
        Manufacturer manufacturer = new() {
            Id = Guid.NewGuid(),
            Userid = user.Id
        };
        Request request = new() {
            Id = Guid.NewGuid(),
            ManufacturerId = manufacturer.Id,
            Manufacturer = manufacturer
        };
        var dbMock = new MktDbContextBuilder()
            .WithRequests(new List<Request>() { request })
            .Build();
        var controller = GetController(dbMock.Object, AuthServiceWithUser(user));
        SetClaims(controller);
        var result = await controller.Accept(request.Id) as RedirectToActionResult;
        Assert.NotNull(result);
        Assert.Equal("Manufacturer", result.ControllerName);
        Assert.Equal("Requests", result.ActionName);
    }

    [Fact]
    public async Task Accept_Request() {
        User user = new() {
            Id = Guid.NewGuid()
        };
        Manufacturer manufacturer = new() {
            Id = Guid.NewGuid(),
            Userid = user.Id
        };
        Request request = new() {
            Id = Guid.NewGuid(),
            ManufacturerId = manufacturer.Id,
            Manufacturer = manufacturer
        };
        var mockRequests = MktDbContextBuilder
            .MockDbSet(new List<Request>() { request });
        var dbMock = new MktDbContextBuilder()
            .WithRequests(mockRequests)
            .Build();
        var mockDate = new Mock<IDateTools>();
        var cancelledDate = DateTime.Now;
        mockDate.Setup(d => d.UtcNow()).Returns(cancelledDate);
        var controller = GetController(
            dbMock.Object, 
            AuthServiceWithUser(user), 
            mockDate.Object
        );
        SetClaims(controller);
        await controller.Accept(request.Id);
        Assert.Equal(cancelledDate, request.Cancelled);
        mockRequests.Verify(r => r.Update(request), 
            Times.Once()
        );
        dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Once()
        );
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