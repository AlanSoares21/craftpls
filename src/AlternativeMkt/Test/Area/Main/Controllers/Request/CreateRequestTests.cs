using System.Linq.Expressions;
using AlternativeMkt.Db;

namespace AlternativeMkt.Tests.Main.Controllers;

public class CreateRequestTests: RequestUtils
{
    [Fact]
    public async Task Set_Request_Price_As_Total_Price() {
        var price = new CraftItemsPrice() {
            Id = Guid.NewGuid(),
            ManufacturerId = Guid.NewGuid(),
            Price = 3,
            TotalPrice = 4
        };

        User requester = new() {
            Id = Guid.NewGuid()
        };

        var mockRequests = MktDbContextBuilder.MockDbSet(new List<Request>());

        var mockDb = new MktDbContextBuilder()
            .WithItemsPrices(new List<CraftItemsPrice>() { price })
            .WithRequests(mockRequests)
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(requester));
        await controller.New(new() {
            PriceId = price.Id
        });
        
        mockRequests.Verify(
            m => 
                m.AddAsync(It.Is(RequestPriceIs(price.TotalPrice)), 
                It.IsAny<CancellationToken>()), 
            Times.Once()
        );
        mockDb.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once()
        );
    }

    Expression<Func<Request, bool>> RequestPriceIs(int price) {
        return (r) => r.Price == price;
    }
}