
using AlternativeMkt.Db;
using AlternativeMkt.Services;

namespace AlternativeMkt.Tests.Builders;

public class PriceServiceBuilder
{
    MktDbContext _db = new MktDbContextBuilder().Build().Object;
    public PriceServiceBuilder WithDb(MktDbContext value) {
        _db = value;
        return this;
    }
    public PriceService Build() {
        return new PriceService(_db);
    }
}