using HomeChef.Application.Features.Auth;
using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Favorites;
using HomeChef.Application.Features.Foods;
using HomeChef.Application.Features.Images;
using HomeChef.Application.Features.Messages;
using HomeChef.Application.Features.Reviews;
using HomeChef.Application.Features.Search;
using HomeChef.Application.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeChef.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<ImagesOptions>(configuration.GetSection(ImagesOptions.SectionName));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IChefService, ChefService>();
        services.AddScoped<IFoodService, FoodService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IImageService, ImageService>();

        return services;
    }
}