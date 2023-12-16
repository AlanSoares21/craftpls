
using AlternativeMkt.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq.EntityFrameworkCore.DbAsyncQueryProvider;
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

    Mock<DbSet<Manufacturer>>? manufacturers;
    public MktDbContextBuilder WithManufacturers(List<Manufacturer> value) {
        manufacturers = MockDbSet(value);
        return this;
    }

    public MktDbContextBuilder WithManufacturers(Mock<DbSet<Manufacturer>> value) {
        manufacturers = value;
        return this;
    }

    Mock<DbSet<CraftItem>> items = MockDbSet(new List<CraftItem>() { });
    public MktDbContextBuilder WithItems(List<CraftItem> value) => 
        WithItems(MockDbSet(value));

    public MktDbContextBuilder WithItems(Mock<DbSet<CraftItem>> value) {
        items = value;
        return this;
    }

    Mock<DbSet<CraftResource>> resources = MockDbSet(new List<CraftResource>() { });
    public MktDbContextBuilder WithResources(List<CraftResource> value) => 
        WithResources(MockDbSet(value));

    public MktDbContextBuilder WithResources(Mock<DbSet<CraftResource>> value) {
        resources = value;
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
        
        DbSet<Manufacturer> manufacturersDbSet = 
            MockDbSet(new List<Manufacturer>() { }).Object;
        if (manufacturers is not null)
                manufacturersDbSet = manufacturers.Object;
        
        DbSet<CraftItem> itemsDbSet = items.Object;
        DbSet<CraftResource> resourcesDbSet = resources.Object;

        var mockDb = new Mock<MktDbContext>(GetConfig());
        mockDb.Setup(db => db.CraftItemsPrices).Returns(priceDbSet);
        mockDb.Setup(db => db.Requests).Returns(requestsDbSet);
        mockDb.Setup(db => db.Manufacturers).Returns(manufacturersDbSet);
        mockDb.Setup(db => db.CraftItems).Returns(itemsDbSet);
        mockDb.Setup(db => db.CraftResources).Returns(resourcesDbSet);
        return mockDb;
    }

    public static Mock<DbSet<T>> MockDbSet<T>(
        List<T> list) where T : class {
        var data = new InMemoryAsyncEnumerable<T>(list.AsQueryable());
        var mockDbSet = new Mock<DbSet<T>>();
        SetQueryableForDbSet(data, mockDbSet);
        mockDbSet.Setup(d => d.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Callback((T value, CancellationToken _) => {
                list.Add(value);
            });
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