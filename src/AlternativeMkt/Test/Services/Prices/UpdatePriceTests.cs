
using AlternativeMkt.Db;
using AlternativeMkt.Models;
using AlternativeMkt.Services;

namespace AlternativeMkt.Tests.Services;

public class UpdatePriceTests: PricesUtils
{
    [Fact]
    public async Task Update_Price() {
        CraftItemsPrice priceUpdated = new() {
            Id = Guid.NewGuid(),
            Price = 1000,
            TotalPrice = 1400
        };
        var mockPrices = MktDbContextBuilder
            .MockDbSet(new List<CraftItemsPrice>());
        var mockDb = new MktDbContextBuilder()
            .WithItemsPrices(mockPrices)
            .Build();
        IPriceService service = new PriceServiceBuilder()
            .WithDb(mockDb.Object)
            .Build();
        UpdateItemPrice data = new() {price = 2000};
        await service.UpdatePrice(priceUpdated, data);
        mockPrices.Verify(p => p.Update(priceUpdated), Times.Once());
        mockDb.Verify(
            d => d.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once()
        );
        Assert.Equal(data.price, priceUpdated.Price);
        Assert.Equal(data.price + 400, priceUpdated.TotalPrice);
    }

    [Fact]
    public async Task When_Update_A_Resource_Price_Update_The_Items_Prices_Rely_In_It() {
        Manufacturer manufacturer = new() {Id = Guid.NewGuid()};
        CraftItem baseItem = new() {Id = 1};
        CraftItemsPrice priceUpdated = GetPrice(
            price: 400,
            totalPrice: 400,
            baseItem,
            manufacturer
        );
        CraftItem firstItemAffected = new() {Id = 2};
        var resourceOne = GetCraftResource(1, 
            firstItemAffected, 
            baseItem
        );
        var priceFirstItem = GetPrice(
            price: 1000,
            totalPrice: 1400,
            firstItemAffected,
            manufacturer
        );
        CraftItem secondItemAffected = new() {Id = 3};
        var resourceTwo = GetCraftResource(2, 
            secondItemAffected, 
            firstItemAffected
        );
        var priceSecondItem = GetPrice(
            price: 1200,
            totalPrice: 2600,
            secondItemAffected,
            manufacturer
        );

        var mockPrices = MktDbContextBuilder
            .MockDbSet(new List<CraftItemsPrice>() {
                priceUpdated,
                priceFirstItem,
                priceSecondItem
            });
        var mockDb = new MktDbContextBuilder()
            .WithItems(new List<CraftItem>() {
                baseItem,
                firstItemAffected,
                secondItemAffected
            })
            .WithResources(new List<CraftResource>() {
                resourceOne, resourceTwo
            })
            .WithManufacturers(new List<Manufacturer>() {manufacturer})
            .WithItemsPrices(mockPrices)
            .Build();
        IPriceService service = new PriceServiceBuilder()
            .WithDb(mockDb.Object)
            .Build();
        UpdateItemPrice data = new() {price = 500};
        await service.UpdatePrice(priceUpdated, data);
        mockPrices.Verify(p => p.Update(priceUpdated), Times.Once(), "base item price was not updated");
        mockPrices.Verify(p => p.Update(priceFirstItem), Times.Once(), "first item price was not updated");
        mockPrices.Verify(p => p.Update(priceSecondItem), Times.Once(), "second item price was not updated");
        mockDb.Verify(
            d => d.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(3),
            "changes was not saved"
        );
        Assert.Equal(data.price, priceUpdated.Price);
        Assert.Equal(data.price, priceUpdated.TotalPrice);

        int expectedPriceForFirstItem = 1500;
        Assert.Equal(1000, priceFirstItem.Price);
        Assert.Equal(expectedPriceForFirstItem, priceFirstItem.TotalPrice);

        int expectedPriceForSecondItem = 2700;
        Assert.Equal(1200, priceSecondItem.Price);
        Assert.Equal(expectedPriceForSecondItem, priceSecondItem.TotalPrice);
    }
}