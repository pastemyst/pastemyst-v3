using Microsoft.EntityFrameworkCore;
using pastemyst.Models;

namespace pastemyst.DbContexts;

public class UserContext : DbContext
{
    public UserContext(DbContextOptions<UserContext> contextOptions) : base(contextOptions)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("citext");

        modelBuilder.Entity<User>()
            .HasIndex(e => e.Username)
            .IsUnique();
    }
}