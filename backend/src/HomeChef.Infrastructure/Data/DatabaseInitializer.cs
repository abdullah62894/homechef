using HomeChef.Domain.Constants;
using HomeChef.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HomeChef.Infrastructure.Data;

/// <summary>
/// Applies pending migrations (when Database:AutoMigrate is enabled) and seeds
/// the well-known roles. Called from the API host at startup.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DatabaseInitializer));

        var dbContext = scope.ServiceProvider.GetRequiredService<HomeChefDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        if (configuration.GetValue<bool>("Database:AutoMigrate"))
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Database migrations applied.");
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        foreach (var role in new[] { Roles.Customer, Roles.Chef, Roles.Admin, Roles.Moderator })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
                logger.LogInformation("Seeded role {Role}.", role);
            }
        }
    }
}