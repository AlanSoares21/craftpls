
using System.Linq.Expressions;
using System.Text.Json;
using AlternativeMkt.Api.Controllers;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Tests.Api.Controllers;

public class AddPricesTests: PricesUtils
{
    [Fact]
    public async Task When_Add_Price_To_An_Unregistered_Item_Return_Not_Found() {
        Manufacturer manufacturer = new() { 
            Id = Guid.NewGuid(), 
            CraftItemsPrices = new List<CraftItemsPrice>() };
        var mockDb = new MktDbContextBuilder()
            .WithManufacturers(new List<Manufacturer>() { manufacturer })
            .WithItemsPrices(new List<CraftItemsPrice>())
            .Build();
        var controller = GetController(mockDb.Object, Utils.AuthServiceWithUser(new()));

        AddItemPrice newPrice = new() { itemId = 0, manufacturerId = manufacturer.Id, price = 1 };
        var result = await controller.Add(newPrice) as BadRequestObjectResult;
        Assert.NotNull(result);
        var errorData = (ApiError?)result.Value;
        Assert.NotNull(errorData);
        Assert.Contains($"Item {newPrice.itemId} not found", errorData.Message);
    }
    
    [Fact]
    public async Task Can_Not_Set_A_Price_Less_Than_One() {
        var mockDb = new MktDbContextBuilder().Build();
        var controller = GetController(mockDb.Object, Utils.AuthServiceWithUser(new()));
        AddItemPrice newPrice = new() { price = 0 };
        var result = await controller.Add(newPrice) as BadRequestObjectResult;
        Assert.NotNull(result);
        var errorData = (ApiError?)result.Value;
        Assert.NotNull(errorData);
        Assert.Contains("Price must be greater than zero", errorData.Message);
    }
    
    [Fact]
    public async Task When_Try_Add_Price_To_A_Item_That_Already_Have_A_Price_Return_Error() {
        var price = new CraftItemsPrice() {
            ItemId = 123
        };
        Manufacturer manufacturer = new() { 
            Id = Guid.NewGuid(), 
            CraftItemsPrices = new List<CraftItemsPrice>() { price } 
        };
        var mockDb = new MktDbContextBuilder()
            .WithManufacturers(new List<Manufacturer>() { manufacturer })
            .WithItemsPrices(new List<CraftItemsPrice>() { price })
            .Build();
        var controller = GetController(mockDb.Object, Utils.AuthServiceWithUser(new()));

        AddItemPrice newPrice = new() { itemId = price.ItemId, manufacturerId = manufacturer.Id, price = 1 };
        var result = await controller.Add(newPrice) as BadRequestObjectResult;
        Assert.NotNull(result);
        var errorData = (ApiError?)result.Value;
        Assert.NotNull(errorData);
        Assert.Contains($"You have alredy set a price for this item {newPrice.itemId}", errorData.Message);
    }

    [Fact]
    public async Task Add_Price() {
        var item = new CraftItem() {
            Id = 123
        };
        User user = new() { Id = Guid.NewGuid() };
        Manufacturer manufacturer = new() { 
            Id = Guid.NewGuid(), 
            CraftItemsPrices = new List<CraftItemsPrice>(),
            Userid = user.Id
        };
        var mockPrices = MktDbContextBuilder.MockDbSet(new List<CraftItemsPrice>());
        var mockDb = new MktDbContextBuilder()
            .WithManufacturers(new List<Manufacturer>() { manufacturer })
            .WithItemsPrices(mockPrices)
            .WithItems(new List<CraftItem>() { item })
            .Build();
        var controller = GetController(mockDb.Object, Utils.AuthServiceWithUser(user));
        AddItemPrice newPrice = new() { 
            itemId = item.Id, 
            manufacturerId = manufacturer.Id, 
            price = 4321 
        };
        var result = await controller.Add(newPrice) as CreatedResult;
        Assert.NotNull(result);
        var resultData = (AddItemPrice?)result.Value;
        Assert.Equal(newPrice, resultData);
        
        mockDb.Verify(d => 
            d.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Once()
        );

        CraftItemsPrice expectedPrice = new() {
            ItemId = item.Id,
            ManufacturerId = manufacturer.Id,
            Price = newPrice.price,
            TotalPrice = newPrice.price,
        };
        mockPrices.Verify(p => 
            p.AddAsync(
                It.Is(PriceMatchProperties(expectedPrice)), 
                It.IsAny<CancellationToken>()
            ),
            Times.Once()
        );
    }

    [Fact]
    public async Task Can_Not_Add_Price_To_An_Item_Before_Add_Prices_To_It_Resources() {
        User user = new() { Id = Guid.NewGuid() };
        Manufacturer manufacturer = new() { 
            Id = Guid.NewGuid(), 
            Userid = user.Id,
            CraftItemsPrices = new List<CraftItemsPrice>()
        };
        var baseItemOne = new CraftItem() {
            Id = 111,
            Prices = new List<CraftItemsPrice>()
        };
        var requestedItem = new CraftItem() {
            Id = 123,
            Resources = new List<CraftResource>()
        };
        var resourceOne = new CraftResource() {
            Id = 11,
            Item = requestedItem,
            ItemId = requestedItem.Id,
            ResourceId = baseItemOne.Id,
            Resource = baseItemOne,
            Amount = 1,
        };
        requestedItem.Resources.Add(resourceOne);
        baseItemOne.ResourceFor = new List<CraftResource>() { resourceOne };
        var mockDb = new MktDbContextBuilder()
            .WithManufacturers(new List<Manufacturer>() { manufacturer })
            .WithItems(new List<CraftItem>() { requestedItem, baseItemOne })
            .WithResources(new List<CraftResource>() { resourceOne })
            .Build();
        var controller = GetController(mockDb.Object, Utils.AuthServiceWithUser(user));
        AddItemPrice newPrice = new() { 
            itemId = requestedItem.Id,
            manufacturerId = manufacturer.Id, 
            price = 9876
        };
        var result = await controller.Add(newPrice) as BadRequestObjectResult;
        Assert.NotNull(result);
        var error = (ApiError?)result.Value;
        Assert.NotNull(error);
        Assert.Contains($"Add prices to item resources before add a price to it", error.Message);
    }

    [Fact]
    public async Task Total_Price_Is_The_Sum_Of_Item_Price_And_It_Resources_Total_Prices() {
        User user = new() { Id = Guid.NewGuid() };
        Manufacturer manufacturer = new() { 
            Id = Guid.NewGuid(), 
            Userid = user.Id,
            CraftItemsPrices = new List<CraftItemsPrice>()
        };
        var baseItemOne = new CraftItem() {
            Id = 111,
            Prices = new List<CraftItemsPrice>()
        };
        var baseItemTwo = new CraftItem() {
            Id = 222,
            Prices = new List<CraftItemsPrice>()
        };
        var priceItemOne = new CraftItemsPrice() {
            Id = Guid.NewGuid(),
            ManufacturerId = manufacturer.Id,
            Manufacturer = manufacturer,
            ItemId = baseItemOne.Id,
            Item = baseItemOne,
            Price = 0,
            TotalPrice = 10
        };
        var priceItemTwo = new CraftItemsPrice() {
            Id = Guid.NewGuid(),
            ManufacturerId = manufacturer.Id,
            Manufacturer = manufacturer,
            ItemId = baseItemTwo.Id,
            Item = baseItemTwo,
            Price = 0,
            TotalPrice = 20
        };
        baseItemOne.Prices.Add(priceItemOne);
        baseItemTwo.Prices.Add(priceItemTwo);
        manufacturer.CraftItemsPrices.Add(priceItemOne);
        manufacturer.CraftItemsPrices.Add(priceItemTwo);
        var requestedItem = new CraftItem() {
            Id = 123,
            Resources = new List<CraftResource>()
        };
        var resourceOne = new CraftResource() {
            Id = 11,
            Item = requestedItem,
            ItemId = requestedItem.Id,
            ResourceId = baseItemOne.Id,
            Resource = baseItemOne,
            Amount = 1,
        };
        var resourceTwo = new CraftResource() {
            Id = 22,
            Item = requestedItem,
            ItemId = requestedItem.Id,
            ResourceId = baseItemTwo.Id,
            Resource = baseItemTwo,
            Amount = 2,
        };
        requestedItem.Resources.Add(resourceOne);
        requestedItem.Resources.Add(resourceTwo);
        baseItemOne.ResourceFor = new List<CraftResource>() { resourceOne };
        baseItemTwo.ResourceFor = new List<CraftResource>() { resourceTwo };

        var mockPrices = MktDbContextBuilder.MockDbSet(manufacturer.CraftItemsPrices.ToList());
        var mockDb = new MktDbContextBuilder()
            .WithManufacturers(new List<Manufacturer>() { manufacturer })
            .WithItemsPrices(mockPrices)
            .WithItems(new List<CraftItem>() { requestedItem, baseItemOne, baseItemTwo })
            .WithResources(new List<CraftResource>() { resourceOne, resourceTwo })
            .Build();
        var controller = GetController(mockDb.Object, Utils.AuthServiceWithUser(user));
        AddItemPrice newPrice = new() { 
            itemId = requestedItem.Id,
            manufacturerId = manufacturer.Id, 
            price = 100
        };
        var result = await controller.Add(newPrice) as CreatedResult;
        Assert.NotNull(result);
        var resultData = (AddItemPrice?)result.Value;
        Assert.Equal(newPrice, resultData);
        
        mockDb.Verify(d => 
            d.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Once()
        );

        CraftItemsPrice expectedPrice = new() {
            ItemId = requestedItem.Id,
            ManufacturerId = manufacturer.Id,
            Price = newPrice.price,
            /*
                TotalPrice = newPrice.price 
                    + priceItemOne.TotalPrice * resourceOne.Ammount
                    + priceItemTwo.TotalPrice * resourceTwo.Ammount
                
                TotalPrice = 100 
                    + 10 * 1
                    + 20 * 2

                TotalPrice = 150
            */
            TotalPrice = 150,
        };
        mockPrices.Verify(p => 
            p.AddAsync(
                It.Is(PriceMatchProperties(expectedPrice)), 
                It.IsAny<CancellationToken>()
            ),
            Times.Once()
        );
    }

    Expression<Func<CraftItemsPrice, bool>> PriceMatchProperties(CraftItemsPrice expected) {

        return c => 
            c.Price == expected.Price
            && c.TotalPrice == expected.TotalPrice
            && c.ItemId == expected.ItemId
            && c.ManufacturerId == expected.ManufacturerId;
    }
}