using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Data;

public class HomeChefDbContext : DbContext
{
    public HomeChefDbContext(DbContextOptions<HomeChefDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("homechef");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HomeChefDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}