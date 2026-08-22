using HomeChef.Domain.Chefs;
using HomeChef.Domain.Favorites;
using HomeChef.Domain.Foods;
using HomeChef.Domain.Identity;
using HomeChef.Domain.Messages;
using HomeChef.Domain.Notifications;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("homechef");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HomeChefDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}