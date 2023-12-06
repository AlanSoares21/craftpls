using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using AlternativeMkt.Models;
using AlternativeMkt.Services;
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
        var mockPriceService = new Mock<IPriceService>();
        var mockDb = new MktDbContextBuilder()
            .WithManufacturers(new List<Manufacturer>() { manufacturer })
            .Build();
        var controller = GetController(
            mockDb.Object, 
            Utils.AuthServiceWithUser(user),
            mockPriceService.Object
        );
        AddItemPrice newPrice = new() { 
            itemId = item.Id, 
            manufacturerId = manufacturer.Id, 
            price = 1
        };
        var result = await controller.Add(newPrice) as CreatedResult;
        Assert.NotNull(result);
        var resultData = (AddItemPrice?)result.Value;
        Assert.Equal(newPrice, resultData);
        mockPriceService.Verify(s => 
            s.AddPrice(newPrice), 
            Times.Once()
        );
    }
}