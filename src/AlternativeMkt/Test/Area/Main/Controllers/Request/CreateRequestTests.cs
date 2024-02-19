using System.Linq.Expressions;
using System.Security.Claims;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Tests.Main.Controllers;

public class CreateRequestTests: RequestUtils
{
    [Fact]
    public async Task When_Item_Price_Not_Found_Return_Error() {
        var mockAuth = new Mock<IAuthService>();
        mockAuth.Setup(a => a.GetUser(It.IsAny<IEnumerable<Claim>>()))
            .Returns(new User());

        var priceList = new List<CraftItemsPrice>() { };
        var mockDb = new MktDbContextBuilder()
            .WithItemsPrices(priceList)
            .Build();
        var controller = GetController(mockDb.Object, mockAuth.Object);
        var result = await controller.New(new()) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Error", result.ViewName);
        var model = (string?) result.Model;
        Assert.Contains("Price not found", model);
    }

    [Fact]
    public async Task Save_New_Request_In_Db() {
        User requester = new() {
            Id = Guid.NewGuid()
        };
        Manufacturer manufacturer = Utils.GetManufacturer();

        CraftItemsPrice itemsPrice = Utils.GetPrice(
            price: 123,
            totalPrice: 123,
            new CraftItem(),
            manufacturer
        ); 
        
        var mockRequests = MktDbContextBuilder.MockDbSet(new List<Request>());

        var mockDb = new MktDbContextBuilder()
            .WithItemsPrices(new List<CraftItemsPrice>() { itemsPrice })
            .WithRequests(mockRequests)
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(requester));
        await controller.New(new() {
            PriceId = itemsPrice.Id
        });
        
        mockRequests.Verify(
            m => 
                m.AddAsync(It.IsAny<Request>(), 
                It.IsAny<CancellationToken>()), 
            Times.Once()
        );
        mockDb.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once()
        );
    }
    
    [Fact]
    public async Task Set_Request_Price_As_Total_Price() {
        Manufacturer manufacturer = Utils.GetManufacturer();
        var price = Utils.GetPrice(
            price: 3,
            totalPrice: 4,
            new CraftItem(),
            manufacturer
        );
        User requester = new() {Id = Guid.NewGuid()};

        var mockRequests = MktDbContextBuilder.MockDbSet(new List<Request>());

        var mockDb = new MktDbContextBuilder()
            .WithItemsPrices(new List<CraftItemsPrice>() { price })
            .WithRequests(mockRequests)
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(requester));
        await controller.New(new() {
            PriceId = price.Id
        });
        
        mockRequests.Verify(
            m => 
                m.AddAsync(It.Is(RequestPriceIs(price.TotalPrice)), 
                It.IsAny<CancellationToken>()), 
            Times.Once()
        );
        mockDb.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once()
        );
    }

    [Fact]
    public async Task Redirect_To_List_After_Create_New_Request() {
        User requester = new() {Id = Guid.NewGuid()};
        Manufacturer manufacturer = Utils.GetManufacturer();
        CraftItemsPrice itemsPrice = Utils.GetPrice(
            price: 1,
            totalPrice: 1,
            new CraftItem(),
            manufacturer
        );
        var mockRequests = MktDbContextBuilder.MockDbSet(new List<Request>());
        var mockDb = new MktDbContextBuilder()
            .WithItemsPrices(new List<CraftItemsPrice>() { itemsPrice })
            .WithRequests(mockRequests)
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(requester));
        var result = await controller.New(new() {
            PriceId = itemsPrice.Id
        }) as RedirectToActionResult;
        Assert.NotNull(result);
        Assert.Equal("List", result.ActionName);
    }

    [Fact]
    public async Task Reduce_Provided_Resources_Prices_From_Request_Price() {
        Manufacturer manufacturer = new() {
            Id = Guid.NewGuid()
        };

        CraftItem firstResourceItem = new() {
            Id = 1
        };
        CraftItem secondResourceItem = new() {
            Id = 2
        };
        CraftItem craftItem = new() {
            Id = 3
        };

        var resource01 = Utils.GetCraftResource(1, craftItem, firstResourceItem);
        var resource02 = Utils.GetCraftResource(2, craftItem, secondResourceItem);
        
        var firstResourcePrice = Utils.GetPrice(
            price: 1, 
            totalPrice: 1,
            firstResourceItem,
            manufacturer
        );
        var secondResourcePrice = Utils.GetPrice(
            price: 2, 
            totalPrice: 2,
            firstResourceItem,
            manufacturer
        );

        var craftItemPrice = Utils.GetPrice(
            price: 3,
            totalPrice: 3 + secondResourcePrice.TotalPrice + firstResourcePrice.TotalPrice,
            craftItem,
            manufacturer
        );

        User requester = new() {
            Id = Guid.NewGuid()
        };

        var mockRequests = MktDbContextBuilder.MockDbSet(new List<Request>());

        var mockDb = new MktDbContextBuilder()
            .WithItems(new List<CraftItem>() {
                firstResourceItem,
                secondResourceItem,
                craftItem
            })
            .WithResources(new List<CraftResource>() {
                resource01,
                resource02
            })
            .WithItemsPrices(new List<CraftItemsPrice>() { 
                firstResourcePrice,
                secondResourcePrice,
                craftItemPrice
             })
            .WithRequests(mockRequests)
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(requester));
        await controller.New(new() {
            PriceId = craftItemPrice.Id,
            ProvidedResources = new() { resource01.Id, resource02.Id }
        });
        
        mockRequests.Verify(
            m => 
                m.AddAsync(It.Is(RequestPriceIs(
                    craftItemPrice.TotalPrice 
                    - firstResourcePrice.TotalPrice
                    - secondResourcePrice.TotalPrice
                )), 
                It.IsAny<CancellationToken>()), 
            Times.Once()
        );
        mockDb.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once()
        );
    }

    Expression<Func<Request, bool>> RequestPriceIs(int price) {
        return (r) => r.Price == price;
    }

    [Fact]
    public async Task Save_Provided_Resources_Of_A_New_Request() {
        Manufacturer manufacturer = new() {
            Id = Guid.NewGuid()
        };

        CraftItem firstResourceItem = new() {
            Id = 1
        };
        CraftItem secondResourceItem = new() {
            Id = 2
        };
        CraftItem craftItem = new() {
            Id = 3
        };

        var resource01 = Utils.GetCraftResource(1, craftItem, firstResourceItem);
        var resource02 = Utils.GetCraftResource(2, craftItem, secondResourceItem);
        
        var firstResourcePrice = Utils.GetPrice(
            price: 1, 
            totalPrice: 1,
            firstResourceItem,
            manufacturer
        );
        var secondResourcePrice = Utils.GetPrice(
            price: 2, 
            totalPrice: 2,
            firstResourceItem,
            manufacturer
        );

        var craftItemPrice = Utils.GetPrice(
            price: 3,
            totalPrice: 3 + secondResourcePrice.TotalPrice + firstResourcePrice.TotalPrice,
            craftItem,
            manufacturer
        );

        User requester = new() {
            Id = Guid.NewGuid()
        };

        var mockRequests = MktDbContextBuilder.MockDbSet(new List<Request>());

        var mockDb = new MktDbContextBuilder()
            .WithItems(new List<CraftItem>() {
                firstResourceItem,
                secondResourceItem,
                craftItem
            })
            .WithResources(new List<CraftResource>() {
                resource01,
                resource02
            })
            .WithItemsPrices(new List<CraftItemsPrice>() { 
                firstResourcePrice,
                secondResourcePrice,
                craftItemPrice
             })
            .WithRequests(mockRequests)
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(requester));
        await controller.New(new() {
            PriceId = craftItemPrice.Id,
            ProvidedResources = new() { resource01.Id, resource02.Id }
        });
        
        mockRequests.Verify(
            m => m.AddAsync(It.Is<Request>(
                r => r.ResourcesProvided.All(rp => 
                    rp.ResourceId == resource01.Id ||
                    rp.ResourceId == resource02.Id
                )
            ), It.IsAny<CancellationToken>()), 
            Times.Once()
        );
        mockDb.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once()
        );
    }

    [Fact]
    public async Task When_Invalid_Resources_Are_Provided_Return_Error() {
        Manufacturer manufacturer = new() {
            Id = Guid.NewGuid()
        };

        CraftItem firstResourceItem = new() {
            Id = 1
        };
        CraftItem notAResourceItem = new() {
            Id = 2
        };
        CraftItem craftItem = new() {
            Id = 3
        };

        var resource01 = Utils.GetCraftResource(1, craftItem, firstResourceItem);
        var invalidResource = Utils.GetCraftResource(2, firstResourceItem, notAResourceItem);
        
        var firstResourcePrice = Utils.GetPrice(
            price: 1, 
            totalPrice: 1,
            firstResourceItem,
            manufacturer
        );
        /*
        var notAResourcePrice = Utils.GetPrice(
            price: 2, 
            totalPrice: 2,
            firstResourceItem,
            manufacturer
        );
        */

        var craftItemPrice = Utils.GetPrice(
            price: 3,
            totalPrice: 3 
            // + secondResourcePrice.TotalPrice 
            + firstResourcePrice.TotalPrice,
            craftItem,
            manufacturer
        );

        User requester = new() {
            Id = Guid.NewGuid()
        };

        var mockRequests = MktDbContextBuilder.MockDbSet(new List<Request>());

        var mockDb = new MktDbContextBuilder()
            .WithItems(new List<CraftItem>() {
                firstResourceItem,
               notAResourceItem,
                craftItem
            })
            .WithResources(new List<CraftResource>() {
                resource01, 
                invalidResource
            })
            .WithItemsPrices(new List<CraftItemsPrice>() { 
                firstResourcePrice,
                craftItemPrice
             })
            .WithRequests(mockRequests)
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(requester));
        var result = await controller.New(new() {
            PriceId = craftItemPrice.Id,
            ProvidedResources = new() { 
                resource01.Id,
                invalidResource.Id
            }
        }) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Error", result.ViewName);
        Assert.Equal("Some of the resources provided are not in the resources list.", result.Model);

        mockRequests.Verify(m => m.AddAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), 
            Times.Never()
        );
        mockDb.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never()
        );
    }

    [Fact]
    public async Task When_The_Requester_Is_The_Manufacturer_Return_Error() {
        User requester = new() {
            Id = Guid.NewGuid()
        };
        Manufacturer manufacturer = new() {
            Id = Guid.NewGuid(),
            User = requester,
            Userid = requester.Id
        };
        CraftItem craftItem = new() {
            Id = 3
        };
        var craftItemPrice = Utils.GetPrice(
            price: 3,
            totalPrice: 3,
            craftItem,
            manufacturer
        );
        var mockRequests = MktDbContextBuilder.MockDbSet(new List<Request>());
        var mockDb = new MktDbContextBuilder()
            .WithManufacturers(new List<Manufacturer>() {manufacturer})
            .WithItems(new List<CraftItem>() {craftItem})
            .WithItemsPrices(new List<CraftItemsPrice>() {craftItemPrice})
            .WithRequests(mockRequests)
            .Build();
        var controller = GetController(mockDb.Object, AuthServiceWithUser(requester));
        var result = await controller.New(new() {
            PriceId = craftItemPrice.Id
        }) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal("Error", result.ViewName);
        Assert.Equal("You can not request items to your self", result.Model);

        mockRequests.Verify(m => m.AddAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), 
            Times.Never()
        );
        mockDb.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never()
        );
    }

}