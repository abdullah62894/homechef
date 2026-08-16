using HomeChef.Domain.Chefs;
using HomeChef.Domain.Identity;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("homechef");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HomeChefDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}