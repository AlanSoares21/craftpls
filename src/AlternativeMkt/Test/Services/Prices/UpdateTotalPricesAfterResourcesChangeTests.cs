using AlternativeMkt.Db;
using AlternativeMkt.Services;

namespace AlternativeMkt.Tests.Services;

public class UpdateTotalPricesAfterResourcesChangeTests: PricesUtils
{
    [Fact]
    public async Task Update_Total_Prices_Of_An_Item_After_A_Resource_Been_Deleted() {
        Manufacturer manufacturer = new() { Id = Guid.NewGuid() };
        CraftItem itemUpdated = new() { Id = 123 };
        CraftItem itemOne = new() { Id = 1 };
        CraftItem itemTwo = new() { Id = 2 };
        CraftResource resourceOne = GetCraftResource(1, itemUpdated, itemOne);
        CraftResource resourceTwo = GetCraftResource(2, itemUpdated, itemTwo);
        CraftItemsPrice priceForItemOne = GetPrice(
            price: 100,
            totalPrice: 100,
            itemOne,
            manufacturer
        );
        CraftItemsPrice priceForItemTwo = GetPrice(
            price: 200,
            totalPrice: 200,
            itemTwo,
            manufacturer
        );
        CraftItemsPrice priceForTheItemUpdated = GetPrice(
            price: 1000,
            totalPrice: 9999,
            itemUpdated,
            manufacturer
        );
        var mockDb = new MktDbContextBuilder()
            .WithItems(new List<CraftItem>() { itemUpdated, itemOne, itemTwo })
            .WithResources(new List<CraftResource>() { resourceOne, resourceTwo })
            .WithManufacturers(new List<Manufacturer>() { manufacturer })
            .WithItemsPrices(new List<CraftItemsPrice>() { priceForItemOne, priceForItemTwo, priceForTheItemUpdated })
            .Build();
        IPriceService service = new PriceServiceBuilder()
            .WithDb(mockDb.Object)
            .Build();
        await service.ResourcesChanged(itemUpdated.Id);
        // int expectedTotalPrice = priceForTheItemUpdated.Price + priceForItemOne.Price + priceForItemTwo.Price;
        int expectedTotalPrice = 1300;
        Assert.Equal(expectedTotalPrice, priceForTheItemUpdated.TotalPrice);
        Assert.True(priceForTheItemUpdated.ResourcesChanged);
        mockDb.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Dont_Use_Prices_From_Different_Manufacturers_To_Calc_The_Total_Price() {
        Manufacturer firstManufacturer = new() { Id = Guid.NewGuid() };
        Manufacturer secondManufacturer = new() { Id = Guid.NewGuid() };
        CraftItem itemUpdated = new() { Id = 123 };
        CraftItem itemOne = new() { Id = 1 };
        CraftResource resourceOne = GetCraftResource(1, itemUpdated, itemOne);
        CraftItemsPrice firstPriceForItemOne = GetPrice(
            price: 100,
            totalPrice: 100,
            itemOne,
            firstManufacturer
        );
        CraftItemsPrice firstPriceForTheItemUpdated = GetPrice(
            price: 1000,
            totalPrice: 9999,
            itemUpdated,
            firstManufacturer
        );

        CraftItemsPrice secondPriceForItemOne = GetPrice(
            price: 1000,
            totalPrice: 1000,
            itemOne,
            secondManufacturer
        );
        CraftItemsPrice secondPriceForTheItemUpdated = GetPrice(
            price: 1000,
            totalPrice: 9999,
            itemUpdated,
            secondManufacturer
        );
        var mockDb = new MktDbContextBuilder()
            .WithItems(new List<CraftItem>() { itemUpdated, itemOne })
            .WithResources(new List<CraftResource>() { resourceOne })
            .WithManufacturers(new List<Manufacturer>() { firstManufacturer, secondManufacturer })
            .WithItemsPrices(new List<CraftItemsPrice>() { firstPriceForItemOne, secondPriceForItemOne, firstPriceForTheItemUpdated, secondPriceForTheItemUpdated })
            .Build();
        IPriceService service = new PriceServiceBuilder()
            .WithDb(mockDb.Object)
            .Build();
        await service.ResourcesChanged(itemUpdated.Id);
        // int expectedTotalPrice = priceForTheItemUpdated.Price + priceForItemOne.Price + priceForItemTwo.Price;
        int expectedTotalPriceForTheFirstManufacturer = 1100;
        int expectedTotalPriceForTheSecondManufacturer = 2000;
        Assert.Equal(
            expectedTotalPriceForTheFirstManufacturer, 
            firstPriceForTheItemUpdated.TotalPrice
        );
        Assert.Equal(
            expectedTotalPriceForTheSecondManufacturer, 
            secondPriceForTheItemUpdated.TotalPrice
        );
        Assert.True(firstPriceForTheItemUpdated.ResourcesChanged);
        Assert.True(secondPriceForTheItemUpdated.ResourcesChanged);
        mockDb.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Dont_Use_Deleted_Prices_To_Calc_The_Total_Price() {
        Manufacturer manufacturer = new() { Id = Guid.NewGuid() };
        CraftItem itemUpdated = new() { Id = 123 };
        CraftItem itemOne = new() { Id = 1 };
        CraftItem itemTwo = new() { Id = 2 };
        CraftResource resourceOne = GetCraftResource(1, itemUpdated, itemOne);
        CraftResource resourceTwo = GetCraftResource(2, itemUpdated, itemTwo);
        CraftItemsPrice priceForItemOne = GetPrice(
            price: 100,
            totalPrice: 100,
            itemOne,
            manufacturer
        );
        CraftItemsPrice deletedPriceForItemTwo = GetPrice(
            price: 300,
            totalPrice: 300,
            itemTwo,
            manufacturer
        );
        deletedPriceForItemTwo.DeletedAt = DateTime.UtcNow;
        CraftItemsPrice priceForItemTwo = GetPrice(
            price: 200,
            totalPrice: 200,
            itemTwo,
            manufacturer
        );
        CraftItemsPrice priceForTheItemUpdated = GetPrice(
            price: 1000,
            totalPrice: 9999,
            itemUpdated,
            manufacturer
        );
        var mockDb = new MktDbContextBuilder()
            .WithItems(new List<CraftItem>() { itemUpdated, itemOne, itemTwo })
            .WithResources(new List<CraftResource>() { resourceOne, resourceTwo })
            .WithManufacturers(new List<Manufacturer>() { manufacturer })
            .WithItemsPrices(new List<CraftItemsPrice>() { priceForItemOne, priceForItemTwo, priceForTheItemUpdated, deletedPriceForItemTwo })
            .Build();
        IPriceService service = new PriceServiceBuilder()
            .WithDb(mockDb.Object)
            .Build();
        await service.ResourcesChanged(itemUpdated.Id);
        // int expectedTotalPrice = priceForTheItemUpdated.Price + priceForItemOne.Price + priceForItemTwo.Price;
        int expectedTotalPrice = 1300;
        Assert.Equal(expectedTotalPrice, priceForTheItemUpdated.TotalPrice);
        Assert.True(priceForTheItemUpdated.ResourcesChanged);
        mockDb.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    
}