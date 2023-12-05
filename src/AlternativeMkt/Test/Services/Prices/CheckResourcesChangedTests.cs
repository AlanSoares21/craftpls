using AlternativeMkt.Db;
using AlternativeMkt.Services;

namespace AlternativeMkt.Tests.Services;

public class CheckResourcesChangedTests: PricesUtils
{
    /*
        - set false propertie resourceschanged
        - dont set propertie 
            - if the price for resource is pendent to check
            - if price to an resource is unseted 
    */
    [Fact]
    public async void Dont_Check_When_A_Price_To_A_Resource_Is_Unchecked() {
        Manufacturer manufacturer = new() { Id = Guid.NewGuid() };
        CraftItem firstItem = new() { Id = 1 };
        CraftItemsPrice priceFirstItem = GetPrice(
            price: 100,
            totalPrice: 100,
            firstItem,
            manufacturer
        );
        CraftItem secondItem = new() { Id = 2 };
        CraftResource firstResource = GetCraftResource(
            1, 
            secondItem, 
            firstItem
        );
        CraftItemsPrice priceSecondItem = GetPrice(
            price: 200,
            totalPrice: 300,
            secondItem,
            manufacturer
        );
        CraftItem thirdItem = new() { Id = 3 };
        CraftResource secondResource = GetCraftResource(
            2, 
            thirdItem, 
            secondItem
        );
        CraftItemsPrice priceThirdItem = GetPrice(
            price: 300,
            totalPrice: 0,
            thirdItem,
            manufacturer
        );
        priceThirdItem.ResourcesChanged = true;
        priceFirstItem.ResourcesChanged = true;
        var mockItemPrices = MktDbContextBuilder.MockDbSet(
            new List<CraftItemsPrice>() { 
                priceFirstItem, 
                priceSecondItem, 
                priceThirdItem 
            }
        );
        var mockDb = new MktDbContextBuilder()
            .WithItems(new List<CraftItem>() { firstItem, secondItem, thirdItem})
            .WithResources(new List<CraftResource>() { firstResource, secondResource })
            .WithItemsPrices(mockItemPrices)
            .WithManufacturers(new List<Manufacturer>() { manufacturer })
            .Build();
        IPriceService service = new PriceServiceBuilder()
            .WithDb(mockDb.Object)
            .Build();
        await Assert.ThrowsAsync<ServiceException>(() => 
            service.CheckResourcesChanged(priceThirdItem)
        );
        mockItemPrices.Verify(p => 
            p.Update(It.IsAny<CraftItemsPrice>()), 
            Times.Never()
        );
    }

    [Fact]
    public async void Dont_Check_When_Dont_Have_A_Price_To_All_Resources() {
        Manufacturer manufacturer = new() { Id = Guid.NewGuid() };
        CraftItem firstItem = new() { Id = 1 };
        CraftItem secondItem = new() { Id = 2 };
        CraftResource firstResource = GetCraftResource(
            1, 
            secondItem, 
            firstItem
        );
        CraftItemsPrice priceSecondItem = GetPrice(
            price: 200,
            totalPrice: 200,
            secondItem,
            manufacturer
        );
        CraftItem thirdItem = new() { Id = 3 };
        CraftResource secondResource = GetCraftResource(
            2, 
            thirdItem, 
            secondItem
        );
        CraftItemsPrice priceThirdItem = GetPrice(
            price: 300,
            totalPrice: 0,
            thirdItem,
            manufacturer
        );
        priceThirdItem.ResourcesChanged = true;
        var mockItemPrices = MktDbContextBuilder.MockDbSet(
            new List<CraftItemsPrice>() {
                priceSecondItem, 
                priceThirdItem 
            }
        );
        var mockDb = new MktDbContextBuilder()
            .WithItems(new List<CraftItem>() { firstItem, secondItem, thirdItem})
            .WithResources(new List<CraftResource>() { firstResource, secondResource })
            .WithItemsPrices(mockItemPrices)
            .WithManufacturers(new List<Manufacturer>() { manufacturer })
            .Build();
        IPriceService service = new PriceServiceBuilder()
            .WithDb(mockDb.Object)
            .Build();
        await Assert.ThrowsAsync<ServiceException>(() => 
            service.CheckResourcesChanged(priceThirdItem)
        );
        mockItemPrices.Verify(p => 
            p.Update(It.IsAny<CraftItemsPrice>()), 
            Times.Never()
        );
    }

    [Fact]
    public async void Check_Resources_Changed_For_A_Price() {
        Manufacturer manufacturer = new() { Id = Guid.NewGuid() };
        CraftItem firstItem = new() { Id = 1 };
        CraftItemsPrice priceFirstItem = GetPrice(
            price: 100,
            totalPrice: 100,
            firstItem,
            manufacturer
        );
        CraftItem secondItem = new() { Id = 2 };
        CraftResource firstResource = GetCraftResource(1, secondItem, firstItem);
        CraftItemsPrice priceSecondItem = GetPrice(
            price: 200,
            totalPrice: 300,
            secondItem,
            manufacturer
        );
        CraftItem thirdItem = new() { Id = 3 };
        CraftResource secondResource = GetCraftResource(2, thirdItem, secondItem);
        CraftItemsPrice priceThirdItem = GetPrice(
            price: 300,
            totalPrice: 600,
            thirdItem,
            manufacturer
        );
        priceThirdItem.ResourcesChanged = true;
        var mockItemPrices = MktDbContextBuilder.MockDbSet(
            new List<CraftItemsPrice>() { 
                priceFirstItem, 
                priceSecondItem, 
                priceThirdItem 
            }
        );
        var mockDb = new MktDbContextBuilder()
            .WithItems(new List<CraftItem>() { firstItem, secondItem, thirdItem})
            .WithResources(new List<CraftResource>() { firstResource, secondResource })
            .WithItemsPrices(mockItemPrices)
            .WithManufacturers(new List<Manufacturer>() { manufacturer })
            .Build();
        IPriceService service = new PriceServiceBuilder()
            .WithDb(mockDb.Object)
            .Build();
        await service.CheckResourcesChanged(priceThirdItem);
        mockItemPrices.Verify(
            p => p.Update(priceThirdItem), 
            Times.Once()
        );
        mockDb.Verify(
            db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Once()
        );
        Assert.False(priceThirdItem.ResourcesChanged);
    }
}