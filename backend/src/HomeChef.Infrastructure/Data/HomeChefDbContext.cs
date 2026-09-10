using HomeChef.Domain.Chefs;
using HomeChef.Domain.Cuisines;
using HomeChef.Domain.Favorites;
using HomeChef.Domain.Foods;
using HomeChef.Domain.Identity;
using HomeChef.Domain.Meals;
using HomeChef.Domain.Messages;
using HomeChef.Domain.Notifications;
using HomeChef.Domain.Orders;
using HomeChef.Domain.Reports;
using HomeChef.Domain.Reviews;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Data;

public class HomeChefDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public HomeChefDbContext(DbContextOptions<HomeChefDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChefProfile> ChefProfiles => Set<ChefProfile>();

    public DbSet<FoodCategory> FoodCategories => Set<FoodCategory>();

    public DbSet<FoodItem> FoodItems => Set<FoodItem>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<FavoriteChef> FavoriteChefs => Set<FavoriteChef>();

    public DbSet<FavoriteFood> FavoriteFoods => Set<FavoriteFood>();

    public DbSet<ChefMessage> ChefMessages => Set<ChefMessage>();

    public DbSet<ContentReport> ContentReports => Set<ContentReport>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<Cuisine> Cuisines => Set<Cuisine>();

    public DbSet<FoodCuisine> FoodCuisines => Set<FoodCuisine>();

    public DbSet<MealCategory> MealCategories => Set<MealCategory>();

    public DbSet<FoodMealCategory> FoodMealCategories => Set<FoodMealCategory>();

    public DbSet<ChefAvailability> ChefAvailabilities => Set<ChefAvailability>();

    public DbSet<ChefDeliveryMethod> ChefDeliveryMethods => Set<ChefDeliveryMethod>();

    public DbSet<FoodAvailability> FoodAvailabilities => Set<FoodAvailability>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("homechef");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HomeChefDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}