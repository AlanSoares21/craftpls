using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AlternativeMkt.Db;

public class MktDbContextFactory : IDesignTimeDbContextFactory<MktDbContext>
{
    public MktDbContext CreateDbContext(string[] args)
    {
        return new MktDbContext(
            new ConfigurationBuilder()
            .AddUserSecrets(System.Reflection.Assembly.GetAssembly(this.GetType()))
            .Build()
        );
    }
}