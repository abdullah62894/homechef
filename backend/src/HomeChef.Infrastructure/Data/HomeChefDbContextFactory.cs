using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HomeChef.Infrastructure.Data;

/// <summary>
/// Design-time factory used by the EF Core CLI (dotnet ef migrations) so migrations
/// can be generated without a running application host. The connection string is read
/// from the ConnectionStrings__Default environment variable, falling back to the local
/// development database for convenience.
/// </summary>
public class HomeChefDbContextFactory : IDesignTimeDbContextFactory<HomeChefDbContext>
{
    private const string FallbackConnectionString =
        "Host=127.0.0.1;Port=5433;Database=homechef;Username=postgres;Password=homechef_dev_pw";

    public HomeChefDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                               ?? FallbackConnectionString;

        var options = new DbContextOptionsBuilder<HomeChefDbContext>()
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "homechef"))
            .Options;

        return new HomeChefDbContext(options);
    }
}