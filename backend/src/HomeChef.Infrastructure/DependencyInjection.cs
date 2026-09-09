using HomeChef.Application.Features.Admin;
using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Favorites;
using HomeChef.Application.Features.Foods;
using HomeChef.Application.Features.Images;
using HomeChef.Application.Features.Messages;
using HomeChef.Application.Features.Notifications;
using HomeChef.Application.Features.Reports;
using HomeChef.Application.Features.Reviews;
using HomeChef.Domain.Identity;
using HomeChef.Infrastructure.Data;
using HomeChef.Infrastructure.Repositories;
using HomeChef.Infrastructure.Storage;
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
        var rawConnectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(rawConnectionString))
        {
            throw new InvalidOperationException("Connection string 'Default' is not configured.");
        }

        var connectionString = NormalizeConnectionString(rawConnectionString);

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
        services.AddSingleton<IImageStorage, LocalImageStorage>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        return services;
    }

    private static string NormalizeConnectionString(string connectionString)
    {
        if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(connectionString);
            var userInfo = uri.UserInfo.Split(':');
            var builder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Database = uri.AbsolutePath.TrimStart('/'),
                Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty,
                Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
                SslMode = Npgsql.SslMode.Require
            };
            return builder.ConnectionString;
        }

        return connectionString;
    }
}