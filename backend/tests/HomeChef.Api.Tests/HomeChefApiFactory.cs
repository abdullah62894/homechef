using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HomeChef.Api.Tests;

/// <summary>
/// Boots the real application against the local test database (homechef_test).
/// The connection string can be overridden with the ConnectionStrings__Default
/// environment variable.
/// </summary>
public class HomeChefApiFactory : WebApplicationFactory<Program>
{
    public const string TestConnectionString =
        "Host=127.0.0.1;Port=5433;Database=homechef_test;Username=postgres;Password=homechef_dev_pw";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                               ?? TestConnectionString;

        builder.UseSetting("ConnectionStrings:Default", connectionString);
    }
}