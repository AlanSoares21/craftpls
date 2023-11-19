using AlternativeMkt.Db;
using AlternativeMkt.Main.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Tests.Main.Controllers;

public class FinishTheRequestTests: RequestUtils
{
    [Fact]
    public async Task When_Try_Finish_A_Not_Registered_Request_Return_Not_Found() {
        var mockDb = new MktDbContextBuilder().Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(new User()));
        var result = await controller.Finished(Guid.NewGuid()) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Error", result.ViewName);
        Assert.Equal("Request not found", (string?)result.Model);
    }
    
    [Fact]
    public async Task Can_Not_Finish_A_Request_When_The_User_Is_Not_Involved() {
        Request request = new() {
            Id = Guid.NewGuid(),
            RequesterId = Guid.NewGuid(),
            ManufacturerId = Guid.NewGuid()
        };
        var mockDb = new MktDbContextBuilder()
            .WithRequests(new List<Request>() { request })
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(new User()));
        var result = await controller.Finished(request.Id) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Error", result.ViewName);
        Assert.Equal("You can not finish this request", (string?)result.Model);
    }
    [Fact]
    public async Task Can_Not_Finish_A_Request_When_The_It_Is_Cancelled() {
        var user = new User() {
            Id = Guid.NewGuid()
        };
        Request request = new() {
            Id = Guid.NewGuid(),
            RequesterId = Guid.NewGuid(),
            ManufacturerId = user.Id,
            Cancelled = DateTime.Now
        };
        var mockDb = new MktDbContextBuilder()
            .WithRequests(new List<Request>() { request })
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(user));
        var result = await controller.Finished(request.Id) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Error", result.ViewName);
        Assert.Equal("The request is cancelled", (string?)result.Model);
    }
    
    [Fact]
    public async Task Finish_The_Request_By_The_Manufacturer() {
        var user = new User() {
            Id = Guid.NewGuid()
        };
        Request request = new() {
            Id = Guid.NewGuid(),
            RequesterId = Guid.NewGuid(),
            ManufacturerId = user.Id,
        };
        DateTime finishedAt = DateTime.Now;
        var mockDate = new Mock<IDateTools>();
        mockDate.Setup(d => d.UtcNow()).Returns(finishedAt);
        var mockRequests = MktDbContextBuilder
            .MockDbSet(new List<Request>() { request });
        var mockDb = new MktDbContextBuilder()
            .WithRequests(mockRequests)
            .Build();
        var controller = GetController(
            mockDb.Object, 
            AuthServiceWithUser(user), 
            mockDate.Object
        );
        var result = await controller.Finished(request.Id) as RedirectToActionResult;
        Assert.NotNull(result);
        Assert.Equal("Manufacturer", result.ControllerName);
        Assert.Equal("Requests", result.ActionName);
        mockRequests.Verify(r => r.Update(request), Times.Once());
        Assert.Equal(finishedAt, request.FinishedByManufacturer);
        Assert.Equal(finishedAt, request.UpdatedAt);
        Assert.Null(request.FinishedByRequester);
        mockDb.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Finish_The_Request_By_The_Requester() {
        var user = new User() {
            Id = Guid.NewGuid()
        };
        Request request = new() {
            Id = Guid.NewGuid(),
            RequesterId = user.Id,
            ManufacturerId = Guid.NewGuid(),
        };
        DateTime finishedAt = DateTime.Now;
        var mockDate = new Mock<IDateTools>();
        mockDate.Setup(d => d.UtcNow()).Returns(finishedAt);
        var mockRequests = MktDbContextBuilder
            .MockDbSet(new List<Request>() { request });
        var mockDb = new MktDbContextBuilder()
            .WithRequests(mockRequests)
            .Build();
        var controller = GetController(
            mockDb.Object, 
            AuthServiceWithUser(user), 
            mockDate.Object
        );
        var result = await controller.Finished(request.Id) as RedirectToActionResult;
        Assert.NotNull(result);
        Assert.Equal("Manufacturer", result.ControllerName);
        Assert.Equal("Requests", result.ActionName);
        mockRequests.Verify(r => r.Update(request), Times.Once());
        Assert.Equal(finishedAt, request.FinishedByRequester);
        Assert.Equal(finishedAt, request.UpdatedAt);
        Assert.Null(request.FinishedByManufacturer);
        mockDb.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }
}