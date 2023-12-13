
using AlternativeMkt.Db;
using AlternativeMkt.Services;
using Microsoft.Extensions.Logging;

namespace AlternativeMkt.Tests.Builders;

public class PriceServiceBuilder
{
    MktDbContext _db = new MktDbContextBuilder().Build().Object;
    public PriceServiceBuilder WithDb(MktDbContext value) {
        _db = value;
        return this;
    }
    public PriceService Build() {
        return new PriceService(
            _db, 
            new Mock<ILogger<PriceService>>().Object
        );
    }
}