using System.Security.Claims;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Tests.Main.Controllers;

public class RequestsTest: RequestUtils
{
    [Fact]
    public void Open_Request()
    {
        var item = new CraftItem() { Id = 1 };
        var price = new CraftItemsPrice() {Id = Guid.NewGuid(), ItemId = item.Id, Item = item};

        var mockDb = new MktDbContextBuilder()
            .WithItems(new List<CraftItem>() {item})
            .WithItemsPrices(new List<CraftItemsPrice>() {price})
            .Build();
            
        var controller = GetController(
            mockDb.Object,
            new Mock<IAuthService>().Object
        );
        var result = controller.Open(price.Id) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Open", result.ViewName);

        var model = (CraftItemsPrice?) result.Model;
        Assert.NotNull(model);
        Assert.Equal(price, model);
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
        await controller.Cancel(request.Id);
        Assert.Equal(updateDate, request.UpdatedAt);
        Assert.Equal(updateDate, request.Cancelled);
        mockRequests.Verify(r => r.Update(request), Times.Once());
        mockDb.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task After_Cancel_Request_Redirect_To_List_Action() {
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
        var result = await controller
            .Cancel(request.Id) as RedirectToActionResult;
        Assert.NotNull(result);
        Assert.Equal("List", result.ActionName);
    }
}