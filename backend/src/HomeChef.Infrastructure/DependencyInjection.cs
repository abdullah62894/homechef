using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Favorites;
using HomeChef.Application.Features.Foods;
using HomeChef.Application.Features.Messages;
using HomeChef.Application.Features.Reviews;
using HomeChef.Domain.Identity;
using HomeChef.Infrastructure.Data;
using HomeChef.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeChef.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Default' is not configured.");
        }

        services.AddDbContext<HomeChefDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "homechef")));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<HomeChefDbContext>();

        services.AddScoped<IChefProfileRepository, ChefProfileRepository>();
        services.AddScoped<IFoodCategoryRepository, FoodCategoryRepository>();
        services.AddScoped<IFoodRepository, FoodRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<IMessageRepository, ChefMessageRepository>();

        return services;
    }
}