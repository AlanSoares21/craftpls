using AlternativeMkt.Models;
using AlternativeMkt.Db;
using AlternativeMkt.Services;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Tests.Services;

public class AddPriceTests: PricesUtils
{
    [Fact]
    public async void Add_Price_In_Db() {
        Manufacturer manufacturer = new() {Id = Guid.NewGuid()};
        CraftItem item = new() {Id = 123};
        var mockPrices = MktDbContextBuilder
            .MockDbSet(new List<CraftItemsPrice>());
        var mockDb = new MktDbContextBuilder()
            .WithItems(new List<CraftItem>() {item})
            .WithItemsPrices(mockPrices)
            .WithManufacturers(new List<Manufacturer>() {manufacturer})
            .Build();

        IPriceService service = new PriceServiceBuilder()
            .WithDb(mockDb.Object)
            .Build();
        AddItemPrice newPrice = new() {
            itemId = item.Id,
            manufacturerId = manufacturer.Id,
            price = 1000
        };
        await service.AddPrice(newPrice);
        PriceAddedInDb(mockDb, mockPrices, newPrice);
        mockPrices.Verify(
            p => p.AddAsync(
                It.Is(TotalPriceEqual(newPrice.price)), 
                It.IsAny<CancellationToken>()
            ), 
            Times.Once()
        );
    }
    
    Expression<Func<CraftItemsPrice, bool>> PriceMatchInputData(
        AddItemPrice data
    ) {

        return p => p.Price == data.price
            && p.ItemId == data.itemId
            && p.ManufacturerId == data.manufacturerId;
    }

    [Fact]
    public async void Do_Not_Add_When_The_Manufacturer_Already_Have_A_Price_For_The_Item() {
        Manufacturer manufacturer = new() {Id = Guid.NewGuid()};
        CraftItem item = new() {Id = 123};
        CraftItemsPrice price = GetPrice(price: 1, totalPrice: 1, 
            item, manufacturer
        );
        var mockPrices = MktDbContextBuilder
            .MockDbSet(new List<CraftItemsPrice>() {price});
        var mockDb = new MktDbContextBuilder()
            .WithItems(new List<CraftItem>() {item})
            .WithItemsPrices(mockPrices)
            .WithManufacturers(new List<Manufacturer>() {manufacturer})
            .Build();

        IPriceService service = new PriceServiceBuilder()
            .WithDb(mockDb.Object)
            .Build();
        AddItemPrice newPrice = new() {
            itemId = item.Id,
            manufacturerId = manufacturer.Id,
            price = 1000
        };
        await Assert.ThrowsAsync<ServiceException>(() => 
            service.AddPrice(newPrice)
        );
        DontAddPriceInDb(mockDb, mockPrices);
    }

    [Fact]
    public async void Do_Not_Add_When_The_Manufacturer_Did_Not_Added_Prices_For_The_Resources() {
        Manufacturer manufacturer = new() {Id = Guid.NewGuid()};
        CraftItem resourceItem = new() {Id = 321};
        CraftItem item = new() {Id = 123};
        CraftResource resource = GetCraftResource(1, item, resourceItem);
        
        var mockPrices = MktDbContextBuilder
            .MockDbSet(new List<CraftItemsPrice>());
        var mockDb = new MktDbContextBuilder()
            .WithItems(new List<CraftItem>() {item})
            .WithResources(new List<CraftResource>() {resource})
            .WithItemsPrices(mockPrices)
            .WithManufacturers(new List<Manufacturer>() {manufacturer})
            .Build();

        IPriceService service = new PriceServiceBuilder()
            .WithDb(mockDb.Object)
            .Build();
        AddItemPrice newPrice = new() {
            itemId = item.Id,
            manufacturerId = manufacturer.Id,
            price = 1000
        };
        await Assert.ThrowsAsync<ServiceException>(() => 
            service.AddPrice(newPrice)
        );
        DontAddPriceInDb(mockDb, mockPrices);
    }

    void DontAddPriceInDb(
        Mock<MktDbContext> mockDb, 
        Mock<DbSet<CraftItemsPrice>> mockPrices) 
    {
        mockPrices.Verify(
            p => p.AddAsync(
                It.IsAny<CraftItemsPrice>(), 
                It.IsAny<CancellationToken>()
            ), 
            Times.Never()
        );
        mockDb.Verify(
            d => d.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never()
        );
    }

    [Fact]
    public async void Add_When_The_Manufacturer_Has_Added_Prices_For_The_Resources() {
        Manufacturer manufacturer = new() {Id = Guid.NewGuid()};
        CraftItem resourceItem = new() {Id = 321};
        CraftItemsPrice resourceItemPrice = GetPrice(price: 0, totalPrice: 10, 
            resourceItem, manufacturer
        );
        CraftItem item = new() {Id = 123};
        CraftResource resource = GetCraftResource(1, item, resourceItem);
        resource.Amount = 2;
        var mockPrices = MktDbContextBuilder
            .MockDbSet(new List<CraftItemsPrice>() {resourceItemPrice});
        var mockDb = new MktDbContextBuilder()
            .WithItems(new List<CraftItem>() {item})
            .WithResources(new List<CraftResource>() {resource})
            .WithItemsPrices(mockPrices)
            .WithManufacturers(new List<Manufacturer>() {manufacturer})
            .Build();

        IPriceService service = new PriceServiceBuilder()
            .WithDb(mockDb.Object)
            .Build();
        AddItemPrice newPrice = new() {
            itemId = item.Id,
            manufacturerId = manufacturer.Id,
            price = 1000
        };
        await service.AddPrice(newPrice);
        PriceAddedInDb(mockDb, mockPrices, newPrice);
        mockPrices.Verify(
            p => p.AddAsync(
                It.Is(TotalPriceEqual(
                    (int)(newPrice.price + (resourceItemPrice.TotalPrice * resource.Amount))
                )), 
                It.IsAny<CancellationToken>()
            ), 
            Times.Once()
        );
    }

    void PriceAddedInDb(Mock<MktDbContext> mockDb, 
        Mock<DbSet<CraftItemsPrice>> mockPrices,
        AddItemPrice inputData) {
        mockPrices.Verify(
            p => p.AddAsync(
                It.Is(PriceMatchInputData(inputData)), 
                It.IsAny<CancellationToken>()
            ), 
            Times.Once()
        );
        mockDb.Verify(
            d => d.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once()
        );
    }

    Expression<Func<CraftItemsPrice, bool>> TotalPriceEqual(int value) {
        return p => p.TotalPrice == value;
    }
}