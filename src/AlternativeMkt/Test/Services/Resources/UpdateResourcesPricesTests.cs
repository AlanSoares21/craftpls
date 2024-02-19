using AlternativeMkt.Db;
using AlternativeMkt.Services;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Tests.Services;

public class UpdateResourcesPricesTests
{
    [Fact]
    public async Task Propragate_The_ResourcesChanged_Event_To_Change_Prices_Of_All_Affected_Items_After_Update_A_Resource() {
        Manufacturer manufacturer = new() { Id = Guid.NewGuid() };
        CraftItem firstItem = new() { Id = 1 };
        CraftItemsPrice priceForFirstItem = GetPrice(
            price: 100,
            totalPrice: 100,
            firstItem,
            manufacturer
        );
        CraftItem secondItem = new() { Id = 2 };
        CraftResource firstResource = GetCraftResource(1, secondItem, firstItem);
        CraftItemsPrice priceForSecondItem = GetPrice(
            price: 100,
            totalPrice: 200,
            secondItem,
            manufacturer
        );
        CraftItem thirdItem = new() { Id = 3 };
        CraftResource secondResource = GetCraftResource(2, thirdItem, secondItem);
        CraftItemsPrice  priceForThirdItem = GetPrice(
            price: 300,
            totalPrice: 500,
            firstItem,
            manufacturer
        );
        var mockPriceService = new Mock<IPriceService>();
        var mockCraftResources = MktDbContextBuilder.MockDbSet(new List<CraftResource>() {
            firstResource, secondResource
        });
        var mockDb = new MktDbContextBuilder()
            .WithResources(mockCraftResources)
            .Build();
        ICraftResourceService service = new CraftResourceServiceBuilder()
            .WithDb(mockDb.Object)
            .WithPriceService(mockPriceService.Object)
            .Build();
        firstResource.Amount = 2;
        await service.UpdateResource(firstResource);
        mockCraftResources.Verify(r => r.Update(firstResource), Times.Once());
        mockDb.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        mockPriceService.Verify(p => p.ResourcesChanged(secondItem.Id), Times.Once());
        mockPriceService.Verify(p => p.ResourcesChanged(thirdItem.Id), Times.Once());
    }

    [Fact]
    public async Task Propragate_The_ResourcesChanged_Event_To_Change_Prices_Of_All_Affected_Items_After_Delete_A_Resource() {
        Manufacturer manufacturer = new() { Id = Guid.NewGuid() };
        CraftItem firstItem = new() { Id = 1 };
        CraftItemsPrice priceForFirstItem = GetPrice(
            price: 100,
            totalPrice: 100,
            firstItem,
            manufacturer
        );
        CraftItem secondItem = new() { Id = 2 };
        CraftResource firstResource = GetCraftResource(1, secondItem, firstItem);
        CraftItemsPrice priceForSecondItem = GetPrice(
            price: 100,
            totalPrice: 200,
            secondItem,
            manufacturer
        );
        CraftItem thirdItem = new() { Id = 3 };
        CraftResource secondResource = GetCraftResource(2, thirdItem, secondItem);
        CraftItemsPrice  priceForThirdItem = GetPrice(
            price: 300,
            totalPrice: 500,
            firstItem,
            manufacturer
        );
        var mockPriceService = new Mock<IPriceService>();
        var mockCraftResources = MktDbContextBuilder.MockDbSet(new List<CraftResource>() {
            firstResource, secondResource
        });
        var mockDb = new MktDbContextBuilder()
            .WithResources(mockCraftResources)
            .Build();
        ICraftResourceService service = new CraftResourceServiceBuilder()
            .WithDb(mockDb.Object)
            .WithPriceService(mockPriceService.Object)
            .Build();
        firstResource.Amount = 2;
        await service.DeleteResource(firstResource);
        mockCraftResources.Verify(r => r.Remove(firstResource), Times.Once());
        mockDb.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        mockPriceService.Verify(p => p.ResourcesChanged(secondItem.Id), Times.Once());
        mockPriceService.Verify(p => p.ResourcesChanged(thirdItem.Id), Times.Once());
    }

    CraftResource GetCraftResource(
        int craftRersourceId, 
        CraftItem item, 
        CraftItem resourceItem) {    
        return Utils.GetCraftResource(craftRersourceId,  item, resourceItem);
    }

    CraftItemsPrice GetPrice(int price, int totalPrice, CraftItem item, Manufacturer manufacturer) {
        return Utils.GetPrice(price, totalPrice, item, manufacturer);
    }
}