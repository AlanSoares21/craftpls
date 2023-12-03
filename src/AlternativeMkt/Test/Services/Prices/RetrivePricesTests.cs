
using AlternativeMkt.Db;
using AlternativeMkt.Services;

namespace AlternativeMkt.Tests.Services;

public class RetrivePricesTests: PricesUtils
{
    [Fact]
    public void List_Prices_For_Item() {
        int itemId = 123;
        var priceToRetrive = new CraftItemsPrice() {
            Id = Guid.NewGuid(),
            ManufacturerId = Guid.NewGuid(),
            ItemId = itemId
        };
        var prices = new List<CraftItemsPrice>() {
            priceToRetrive,
            new CraftItemsPrice() {
                Id = Guid.NewGuid(),
                ManufacturerId = Guid.NewGuid(),
                ItemId = itemId,
                DeletedAt = DateTime.UtcNow
            },
            new CraftItemsPrice() {
                Id = Guid.NewGuid(),
                ManufacturerId = Guid.NewGuid(),
                ItemId = itemId,
                ResourcesChanged = true
            }
        };
        var mockDb = new MktDbContextBuilder()
            .WithItemsPrices(prices)
            .Build();
        IPriceService service = new PriceServiceBuilder()
            .WithDb(mockDb.Object)
            .Build();
        List<CraftItemsPrice> pricesRetrived = service.GetPricesForItem(itemId);
        Assert.Equal(1, pricesRetrived.Count);
        Assert.Equal(priceToRetrive, pricesRetrived[0]);
    }
}