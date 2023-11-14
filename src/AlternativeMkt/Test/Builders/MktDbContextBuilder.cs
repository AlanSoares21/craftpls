
using AlternativeMkt.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit.Sdk;

namespace AlternativeMkt.Tests.Builders;

public class MktDbContextBuilder
{
    Mock<DbSet<CraftItemsPrice>>? prices;
    public MktDbContextBuilder WithItemsPrices(List<CraftItemsPrice> value) {
        prices = MockDbSet(value);
        return this;
    }

    public MktDbContextBuilder WithItemsPrices(Mock<DbSet<CraftItemsPrice>> value) {
        prices = value;
        return this;
    }

    Mock<DbSet<Request>>? requests;
    public MktDbContextBuilder WithRequests(List<Request> value) {
        requests = MockDbSet(value);
        return this;
    }

    public MktDbContextBuilder WithRequests(Mock<DbSet<Request>> value) {
        requests = value;
        return this;
    }

    public Mock<MktDbContext> Build() {
        DbSet<CraftItemsPrice> priceDbSet = 
            MockDbSet(new List<CraftItemsPrice>() { }).Object;
        if (prices is not null)
                priceDbSet = prices.Object;

        DbSet<Request> requestsDbSet = 
            MockDbSet(new List<Request>() { }).Object;
        if (requests is not null)
                requestsDbSet = requests.Object;

        var mockDb = new Mock<MktDbContext>(GetConfig());
        mockDb.Setup(db => db.CraftItemsPrices).Returns(priceDbSet);
        mockDb.Setup(db => db.Requests).Returns(requestsDbSet);
        return mockDb;
    }

    public static Mock<DbSet<T>> MockDbSet<T>(
        List<T> list) where T : class {
        var data = list.AsQueryable();
        var mockDbSet = new Mock<DbSet<T>>();
        SetQueryableForDbSet(data, mockDbSet);
        return mockDbSet;
    }

    private static void SetQueryableForDbSet<T>(IQueryable<T> q, Mock<DbSet<T>> mockDbSet) where T : class
    {
        mockDbSet.As<IQueryable<T>>()
            .Setup(p => p.Provider)
            .Returns(q.Provider);
        mockDbSet.As<IQueryable<T>>()
            .Setup(p => p.Expression)
            .Returns(q.Expression);
        mockDbSet.As<IQueryable<T>>()
            .Setup(p => p.ElementType)
            .Returns(q.ElementType);
        mockDbSet.As<IQueryable<T>>()
            .Setup(p => p.GetEnumerator())
            .Returns(() => q.GetEnumerator());
    }

    IConfiguration GetConfig() => new Mock<IConfiguration>().Object;
}