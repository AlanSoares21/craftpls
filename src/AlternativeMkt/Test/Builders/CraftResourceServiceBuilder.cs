
using AlternativeMkt.Db;
using AlternativeMkt.Services;

namespace AlternativeMkt.Tests.Builders;

public class CraftResourceServiceBuilder
{
    MktDbContext _db = new MktDbContextBuilder().Build().Object;
    public CraftResourceServiceBuilder WithDb(MktDbContext value) {
        _db = value;
        return this;
    }
    IPriceService _priceService = new Mock<IPriceService>().Object;
    public CraftResourceServiceBuilder WithPriceService(IPriceService value) {
        _priceService = value;
        return this;
    }
    public CraftResourceService Build() {
        return new CraftResourceService(_db, _priceService);
    }
}