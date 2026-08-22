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

        await SeedAdminAsync(scope.ServiceProvider, configuration, logger, cancellationToken);
    }

    /// <summary>
    /// Admin bootstrap: creates the first admin from Admin:SeedAdminEmail /
    /// Admin:SeedAdminPassword when configured, and promotes any existing
    /// account listed in Admin:Emails to the Admin role.
    /// </summary>
    private static async Task SeedAdminAsync(
        IServiceProvider provider,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        var seedEmail = configuration["Admin:SeedAdminEmail"];
        if (!string.IsNullOrWhiteSpace(seedEmail))
        {
            seedEmail = seedEmail.Trim();
            if (await userManager.FindByEmailAsync(seedEmail) is null)
            {
                var seedPassword = configuration["Admin:SeedAdminPassword"];
                if (string.IsNullOrWhiteSpace(seedPassword))
                {
                    logger.LogWarning("Admin:SeedAdminEmail is set without Admin:SeedAdminPassword; skipping admin seed.");
                }
                else
                {
                    var admin = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        UserName = seedEmail,
                        Email = seedEmail,
                        EmailConfirmed = true,
                        FirstName = "Admin",
                        LastName = "User",
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow,
                    };

                    var created = await userManager.CreateAsync(admin, seedPassword);
                    if (!created.Succeeded)
                    {
                        logger.LogWarning("Admin seed for {Email} failed: {Errors}",
                            seedEmail, string.Join(" ", created.Errors.Select(e => e.Description)));
                    }
                    else
                    {
                        await userManager.AddToRoleAsync(admin, Roles.Admin);
                        logger.LogInformation("Seeded admin account {Email}.", seedEmail);
                    }
                }
            }
        }

        var emails = configuration.GetSection("Admin:Emails").Get<string[]>() ?? [];
        foreach (var email in emails.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()))
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                continue;
            }

            if (!await userManager.IsInRoleAsync(user, Roles.Admin))
            {
                await userManager.AddToRoleAsync(user, Roles.Admin);
                logger.LogInformation("Promoted {Email} to Admin.", email);
            }
        }

        await Task.CompletedTask;
    }
}