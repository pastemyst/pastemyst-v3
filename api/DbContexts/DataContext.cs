using Microsoft.EntityFrameworkCore;
using pastemyst.Models;

namespace pastemyst.DbContexts;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Image> Images { get; set; } = null!;

    public DataContext(DbContextOptions<DataContext> contextOptions) : base(contextOptions)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("citext");

        modelBuilder.Entity<User>()
            .HasIndex(e => e.Username)
            .IsUnique();
    }
}