using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AlternativeMkt.Db;

public class MktDbContextFactory : IDesignTimeDbContextFactory<MktDbContext>
{
    public MktDbContext CreateDbContext(string[] args)
    {
        var assembly = System.Reflection.Assembly.GetAssembly(this.GetType());
        if (assembly is null)
            throw new Exception("Could not use reflection to get assembly data");
        return new MktDbContext(
            new ConfigurationBuilder()
            .AddUserSecrets(assembly)
            .Build()
        );
    }
}